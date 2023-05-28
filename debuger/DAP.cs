using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Collections;

namespace debuger
{
    class DAP
    {
        FTDI ftdi;
        Queue<DAPCMDPacket> cmd_queue;
        Mutex queue_mutex;
        Thread do_cmd_thread;
        bool do_cmd_thread_run;

        enum IRSel
        {
            ABORT = 0x8,
            DPACC = 0xa,
            APACC = 0xb,
            IDCODE = 0xe,
            BYPASS = 0xf,
            APDBUF = 0x10,
            APRBUF = 0x11,
            APWBUF = 0x12,
        }
        enum APSel
        {
            APB_AP = 0x0,
            AHB_AP = 0x1,
            AXI_AP = 0x2,
            JTAG_AP = 0x3
        }
        enum DPRSel
        {
            CTRL_STAT = 0x4,
            SELECT = 0x8,
            RDBUFF = 0xc
        }
        enum APRSel
        {
            CSW = 0x0,
            TAR = 0x4,
            DRW = 0xc
        }
        public enum DSize
        {
            BYTE = 0,
            HWORD = 1,
            WORD = 2,
            DWORD = 3
        }
        struct MemAPReg
        {
            public Int32 prot;
            public Int32 size;
            public Int32 addrinc;
            public Int32 sector;
        }
        struct Memo
        {
            public IRSel ir;
            public APSel apsel;
            public Int32 apaddrh;
            public MemAPReg apbap_reg;
            public MemAPReg axiap_reg;
        }
        public enum DAPCMD
        {
            Reset,
            APBWrite,
            APBRead,
            AXIWrite,
            AXIRead,
            AXIReadBatch,
            AXIWriteBatch,
            Test
        }
        public struct DAPCMDPacket
        {
            public DAPCMD cmd;
            public Int32[] dbuff;
            public Int32[] rbuff;
            public Int32 len;
            public Int32 size;
            public Int32 addr;
            public ref_bool done;
        }
        public class ref_bool
        {
            public bool val { get; set; }
        }

        Memo memo;
        byte[] OutputBuffer = new byte[1024]; // Buffer to hold MPSSE commands and data to be sent to the FT2232H
        byte[] InputBuffer = new byte[1024];  // Buffer to hold data read from the FT2232H
        UInt32 NumBytesToSend = 0;            // Index to the output buffer
        UInt32 NumBytesSent = 0;              // Count of actual bytes sent - used with FT_Write
        UInt32 NumBytesToRead = 0;            // Number of bytes available to read in the driver's input buffer
        UInt32 NumBytesRead = 0;              // Count of actual bytes read - used with FT_Read


        public DAP()
        {
            ftdi = new FTDI();
            cmd_queue = new Queue<DAPCMDPacket>();
            queue_mutex = new Mutex();
            do_cmd_thread = new Thread(do_cmd);
            do_cmd_thread_run = true;
        }
        ~DAP()
        {
            if (do_cmd_thread.ThreadState == ThreadState.Running)
            {
                do_cmd_thread_run = false;
                do_cmd_thread.Join();
            }
        }
        private FTDI.FT_STATUS _send_txbuff()
        {
            FTDI.FT_STATUS ftStatus;
            ftStatus = ftdi.Write(OutputBuffer, NumBytesToSend, ref NumBytesSent);
            NumBytesToSend = 0; // Reset output buffer pointer
            return ftStatus;
        }
        private FTDI.FT_STATUS _read_rxbuff(Int32 len=0)
        {
            FTDI.FT_STATUS ftStatus;
            Int32 retry = 0;
            do
            {
                // Get the number of bytes in the device input buffer
                ftStatus = ftdi.GetRxBytesAvailable(ref NumBytesToRead);
                retry++;
                if (retry > 5000)
                {
                    MessageBox.Show("Get input buffer timeout");
                    ftdi.Close();
                    return FTDI.FT_STATUS.FT_OTHER_ERROR;
                }
            } while (((len == 0 && NumBytesToRead == 0) || (len != 0 && NumBytesToRead != len)) && (ftStatus == FTDI.FT_STATUS.FT_OK));
            // Read out the data from input buffer
            return ftStatus |= ftdi.Read(InputBuffer, NumBytesToRead, ref NumBytesRead);
        }
        public void run()
        {
            UInt32 i;
            FTDI.FT_STATUS ftStatus; // Result of each D2XX call
            UInt32 dwNumDevs = 0; // The number of devices
            FTDI.FT_DEVICE_INFO_NODE[] devicelist = new FTDI.FT_DEVICE_INFO_NODE[10];
            // unsigned int uiDevIndex = 0xF; // The device in the list that is used

            UInt32 ClockDivisor = 2;             // Value of clock divisor, SCL Frequency = 60/((1+div)*2) (MHz) = 20khz

            ftStatus = ftdi.GetNumberOfDevices(ref dwNumDevs);
            if (ftStatus != FTDI.FT_STATUS.FT_OK) return;
            ftStatus = ftdi.GetDeviceList(devicelist);
            if (ftStatus != FTDI.FT_STATUS.FT_OK) return;
            if (dwNumDevs == 0)
            {
                MessageBox.Show("Cannot find any device!!");
                return;
            }
            for (i = 0; i < dwNumDevs; ++i)
            {
                if (devicelist[i].Type == FTDI.FT_DEVICE.FT_DEVICE_232H)
                {
                    ftStatus = ftdi.OpenByIndex(i);
                    if (ftStatus != FTDI.FT_STATUS.FT_OK) return;
                    break;
                }
                else if (i == dwNumDevs - 1)
                {
                    MessageBox.Show("Cannot find any device!!");
                    return;
                }
            }
            // Reset USB device
            ftStatus = ftdi.ResetDevice();
            // Purge USB receive buffer first by reading out all old data from FT232H receive buffer
            ftStatus |= ftdi.GetRxBytesAvailable(ref NumBytesToRead);
            if ((ftStatus == FTDI.FT_STATUS.FT_OK) && NumBytesToRead > 0)
                ftStatus |= ftdi.Read(InputBuffer, NumBytesToRead, ref NumBytesRead);

            // Set USB request transfer sizes to 64K
            ftStatus |= ftdi.InTransferSize(65535);
            // Disable event and error characters
            ftStatus |= ftdi.SetCharacters(0, false, 0, false);
            // Set the read and write timeouts in milliseconds
            ftStatus |= ftdi.SetTimeouts(0, 5000);
            // Set the latency timer (default is 16 msec)
            ftStatus |= ftdi.SetLatency(16);
            // Reset controller
            ftStatus |= ftdi.SetBitMode(0x0, 0x0);
            // Enable MPSSE mode
            ftStatus |= ftdi.SetBitMode(0x0, FTDI.FT_BIT_MODES.FT_BIT_MODE_MPSSE);
            if (ftStatus != FTDI.FT_STATUS.FT_OK) return;

            Thread.Sleep(50); // Wait for all the USB stuff to complete and work

            // -----------------------------------------------------------
            // At this point, the MPSSE is ready for commands
            // -----------------------------------------------------------
            // -----------------------------------------------------------
            // Synchronize the MPSSE by sending a bogus opcode (0xAA),
            // The MPSSE will respond with "Bad Command" (0xFA) followed by
            // the bogus opcode itself.
            // -----------------------------------------------------------

            OutputBuffer[NumBytesToSend++] = 0xAA; // '\xAA';
                                                   // Add bogus command ‘xAA’ to the queue
                                                   // Send off the BAD commands
            _send_txbuff();

            bool CommandEchod = false;
            // Read out the data from input buffer
            ftStatus |= _read_rxbuff();
            // Check if Bad command and echo command received
            for (i = 0; i < NumBytesRead - 1; i++)
            {
                if ((InputBuffer[i] == 0xFA) && (InputBuffer[i + 1] == 0xAA))
                {
                    CommandEchod = true;
                    break;
                }
            }
            if (CommandEchod == false)
            {
                MessageBox.Show("Error in synchronizing the MPSSE");
                ftdi.Close();
                return;
            }

            // -----------------------------------------------------------
            // Configure the MPSSE settings for JTAG
            // Multple commands can be sent to the MPSSE with one FT_Write
            // -----------------------------------------------------------
            NumBytesToSend = 0; // Start with a fresh index
                                // Set up the Hi-Speed specific commands for the FTx232H
                                // Use 60MHz master clock (disable divide by 5)
            OutputBuffer[NumBytesToSend++] = 0x8A;
            // Turn off adaptive clocking (may be needed for ARM)
            OutputBuffer[NumBytesToSend++] = 0x97;
            // Disable three-phase clocking
            OutputBuffer[NumBytesToSend++] = 0x8D;

            // Set initial states of the MPSSE interface - low byte, both pin directions and output values
            //    Pin name  Signal  Direction  Config  Initial State  Config
            //    ADBUS0    TCK     output     1       low            0
            //    ADBUS1    TDI     output     1       low            0
            //    ADBUS2    TDO     input      0                      0
            //    ADBUS3    TMS     output     1       high           1
            //    ADBUS4    GPIOL0  input      0                      0
            //    ADBUS5    GPIOL1  input      0                      0
            //    ADBUS6    GPIOL2  input      0                      0
            //    ADBUS7    GPIOL3  input      0                      0
            // Set data bits low-byte of MPSSE port
            OutputBuffer[NumBytesToSend++] = 0x80;
            // Initial state config above
            OutputBuffer[NumBytesToSend++] = 0x08;
            // Direction config above
            OutputBuffer[NumBytesToSend++] = 0x0B;

            // Set initial states of the MPSSE interface - high byte, both pin directions and output values
            //    Pin name  Signal  Direction  Config  Initial State  Config
            //    ACBUS0    GPIOH0  input      0                      0
            //    ACBUS1    GPIOH1  input      0                      0
            //    ACBUS2    GPIOH2  input      0                      0
            //    ACBUS3    GPIOH3  input      0                      0
            //    ACBUS4    GPIOH4  input      0                      0
            //    ACBUS5    GPIOH5  input      0                      0
            //    ACBUS6    GPIOH6  input      0                      0
            //    ACBUS7    GPIOH7  input      0                      0

            // Set data bits low-byte of MPSSE port
            OutputBuffer[NumBytesToSend++] = 0x82;
            // Initial state config above
            OutputBuffer[NumBytesToSend++] = 0x00;
            // Direction config above
            OutputBuffer[NumBytesToSend++] = 0x00;

            // Set TCK frequency
            // TCK = 60MHz / ((1 + [(1 + 0xValueH * 256) OR 0xValueL]) * 2)
            // Command to set clock divisor
            OutputBuffer[NumBytesToSend++] = 0x86;
            // Set 0xValueL of clock divisor
            OutputBuffer[NumBytesToSend++] = (byte)(ClockDivisor & 0xFF);
            // Set 0xValueH of clock divisor
            OutputBuffer[NumBytesToSend++] = (byte)((ClockDivisor >> 8) & 0xFF);
            // Send off the clock divisor commands
            // ftStatus = ftdi.Write(OutputBuffer, NumBytesToSend, ref NumBytesSent);
            // NumBytesToSend = 0; // Reset output buffer pointer

            // Disable internal loop-back
            // Disable loopback
            OutputBuffer[NumBytesToSend++] = 0x85;
            // Send off the loopback command
            _send_txbuff();

            _jtag_reset();
            do_cmd_thread.Start();
        }
        public void close()
        {
            do_cmd_thread_run = false;
        }
        private void _jtag_tdi_tms(Int32 pat)
        {
            // FTDI.FT_STATUS ftStatus; // Result of each D2XX call
            // byte[] OutputBuffer = new byte[1024]; // Buffer to hold MPSSE commands and data to be sent to the FT2232H
            // UInt32 NumBytesToSend = 0;            // Index to the output buffer
            // UInt32 NumBytesSent = 0;              // Count of actual bytes sent - used with FT_Write
            // 
            // if (!ftdi.IsOpen) return;

            OutputBuffer[NumBytesToSend++] = 0x6B;
            OutputBuffer[NumBytesToSend++] = 0;
            OutputBuffer[NumBytesToSend++] = (byte)((pat & 0x1) << 7 | 0x3);
            // // Send off the TMS command
            // ftStatus = ftdi.Write(OutputBuffer, NumBytesToSend, ref NumBytesSent);
        }
        private void _jtag_tms(Int32 cycle, Int32 pat)
        {
            // FTDI.FT_STATUS ftStatus; // Result of each D2XX call
            // byte[] OutputBuffer = new byte[1024]; // Buffer to hold MPSSE commands and data to be sent to the FT2232H
            // UInt32 NumBytesToSend = 0;            // Index to the output buffer
            // UInt32 NumBytesSent = 0;              // Count of actual bytes sent - used with FT_Write

            if (cycle <= 0 || cycle > 8)
            {
                MessageBox.Show("Send jtag tms fail");
                return;
            }
            // if (!ftdi.IsOpen) return;
            OutputBuffer[NumBytesToSend++] = 0x4B;
            OutputBuffer[NumBytesToSend++] = (byte)(cycle - 1);
            OutputBuffer[NumBytesToSend++] = (byte)pat;
            // // Send off the TMS command
            // ftStatus = ftdi.Write(OutputBuffer, NumBytesToSend, ref NumBytesSent);
        }
        private void _jtag_tdi(Int32 cycle, Int32 pat)
        {
            // FTDI.FT_STATUS ftStatus; // Result of each D2XX call
            // byte[] OutputBuffer = new byte[1024]; // Buffer to hold MPSSE commands and data to be sent to the FT2232H
            // UInt32 NumBytesToSend = 0;            // Index to the output buffer
            // UInt32 NumBytesSent = 0;              // Count of actual bytes sent - used with FT_Write

            if (cycle <= 0 || cycle > 8)
            {
                MessageBox.Show("Send jtag tms fail");
                return;
            }
            // if (!ftdi.IsOpen) return;
            OutputBuffer[NumBytesToSend++] = 0x3B;
            OutputBuffer[NumBytesToSend++] = (byte)(cycle - 1);
            OutputBuffer[NumBytesToSend++] = (byte)pat;
            // // Send off the TMS command
            // ftStatus = ftdi.Write(OutputBuffer, NumBytesToSend, ref NumBytesSent);
        }
        private Int32 _jtag_rw_ir(IRSel inst)
        {
            // Check whether IR already set the value we want
            if (memo.ir == inst)
                return 1;

            // Goto shift_IR
            _jtag_tms(4, 0x03);
            // shift_IR LSB 4 bits
            _jtag_tdi(4, inst.GetHashCode() & 0xf);
            // shift_IR MSB and exit-IR
            _jtag_tdi_tms(inst.GetHashCode() >> 4);

            // Goto idle
            _jtag_tms(2, 0x1);

            _send_txbuff();
            _read_rxbuff(2);
            memo.ir = inst;
            return InputBuffer[NumBytesRead - 1] >> 4;
        }
        private Int64 _jtag_rw_dr(IRSel inst, Int64 val, Int32 len)
        {
            Int64 res = 0;
            Int32 _len;

            _jtag_rw_ir(inst);

            // Goto shift_DR
            _jtag_tms(3, 0x1);

            // shift_DR LSB (len - 1) bits
            for (int i = 0; i < len - 1; i += 8)
            {
                _len = (len - 1 - i) > 8 ? 8 : (len - 1 - i);
                _jtag_tdi(_len, (Int32)(val & ((1 << _len) - 1)));
                val >>= _len;
            }
            // shift_DR MSB and exit-DR
            _jtag_tdi_tms((Int32)val);

            // Goto idle
            _jtag_tms(2, 0x01);

            _send_txbuff();
            _read_rxbuff((len + 6) / 8 + 1);
            
            Int32 sft = 0;
            for (int i = 0; i < (len + 6) / 8 - 1; ++i, sft += 8)
            {
                res |= (Int64)InputBuffer[i] << sft;
            }
            res |= ((Int64)InputBuffer[NumBytesRead - 1] << sft) >> (7 - (len - 1) % 8);

            return res;
        }
        private void _jtag_wr_dr(IRSel inst, Int32[] buff, Int32 len)
        {
            Int64 res = 0;
            Int32 _len;
            Int32 val = 0;
            Int32 i;

            _jtag_rw_ir(inst);

            // Goto shift_DR
            _jtag_tms(3, 0x1);

            // shift_DR LSB (len - 1) bits
            for (i = 0; i < len - 1; i += _len)
            {
                if ((i & 0x1f) == 0)
                    val = buff[i / 32];
                _len = (len - 1 - i) > 8 ? 8 : (len - 1 - i);
                _jtag_tdi(_len, (val & ((1 << _len) - 1)));
                val >>= _len;
            }
            try
            {
                if ((i & 0x1f) == 0) val = buff[i / 32];
            }
            catch { }
            // shift_DR MSB and exit-DR
            _jtag_tdi_tms((Int32)val);

            // Goto idle
            _jtag_tms(2, 0x01);

            _send_txbuff();
        }
        private void _jtag_wr_dr(IRSel inst, Int64 val, Int32 len)
        {
            Int64 res = 0;
            Int32 _len;

            _jtag_rw_ir(inst);

            // Goto shift_DR
            _jtag_tms(3, 0x1);

            // shift_DR LSB (len - 1) bits
            for (int i = 0; i < len - 1; i += 8)
            {
                _len = (len - 1 - i) > 8 ? 8 : (len - 1 - i);
                _jtag_tdi(_len, (Int32)(val & ((1 << _len) - 1)));
                val >>= _len;
            }
            // shift_DR MSB and exit-DR
            _jtag_tdi_tms((Int32)val);

            // Goto idle
            _jtag_tms(2, 0x01);

            _send_txbuff();
        }
        private Int32 _jtag_dpacc_wr(DPRSel addr, Int32 data)
        {
            Int64 res;
            do
            {
                res = _jtag_rw_dr(IRSel.DPACC, ((Int64)data) << (2 + 1) | ((Int64)addr) >> 1 | 0, 35);
            }
            while ((res & 0x7) == 1); // Check wait
            return (Int32)(res >> 3);
        }
        private Int32 _jtag_apacc_wr(APRSel addr, Int32 data)
        {
            Int64 res;
            do
            {
                res = _jtag_rw_dr(IRSel.APACC, ((Int64)data) << (2 + 1) | ((Int64)addr) >> 1 | 0, 35);
            }
            while ((res & 0x7) == 1); // Check wait
            return (Int32)(res >> 3);
        }
        private Int32 _jtag_dpacc_rd(DPRSel addr, Int32 data = 0)
        {
            Int64 res;
            do
            {
                res = _jtag_rw_dr(IRSel.DPACC, ((Int64)data) << (2 + 1) | ((Int64)addr) >> 1 | 1, 35);
            }
            while ((res & 0x7) == 1); // Check wait
            return (Int32)(res >> 3);
        }
        private Int32 _jtag_apacc_rd(APRSel addr, Int32 data = 0)
        {
            Int64 res;
            do
            {
                res = _jtag_rw_dr(IRSel.APACC, ((Int64)data) << (2 + 1) | ((Int64)addr) >> 1 | 1, 35);
            }
            while ((res & 0x7) == 1); // Check wait
            return (Int32)(res >> 3);
        }
        private void _jtag_apdbuf_rd(Int32 len, Int32[] buff)
        {
            if (len > 256 / 4) return;
            _jtag_wr_dr(IRSel.APDBUF, 0, len * 4 * 8 + 1);
            _read_rxbuff(len * 4 + 1);
            
            for (int i = 0; i < len; ++i)
            {
                buff[i] = ((Int32)InputBuffer[i * 4 + 3] << 24) | ((Int32)InputBuffer[i * 4 + 2] << 16) | ((Int32)InputBuffer[i * 4 + 1] << 8) | ((Int32)InputBuffer[i * 4] << 0);
            }
        }
        private void _jtag_aprbuf_rd(Int32 len, Int32[] buff)
        {
            if (len > 256 / 4) return;
            _jtag_wr_dr(IRSel.APRBUF, 0, len * 2 + 1);
            _read_rxbuff(len / 4 + 1);

            for (int i = 0; i < len/16; ++i)
            {
                buff[i] = ((Int32)InputBuffer[i * 4 + 3] << 24) | ((Int32)InputBuffer[i * 4 + 2] << 16) | ((Int32)InputBuffer[i * 4 + 1] << 8) | ((Int32)InputBuffer[i * 4] << 0);
            }
        }
        private void _jtag_apwbuf_wr(Int32 len, Int32[] buff)
        {
            if (len > 256 / 4) return;
            _jtag_wr_dr(IRSel.APWBUF, buff, len * 4 * 8);
            _read_rxbuff(len * 4 + 1);
        }
        private Int32 _jtag_apb_rd(Int32 addr)
        {
            if (memo.apsel != APSel.APB_AP || memo.apaddrh != 0)
            {
                _jtag_dpacc_wr(DPRSel.SELECT, APSel.APB_AP.GetHashCode() << 24 | 0 << 4);
                memo.apsel = APSel.APB_AP;
                memo.apaddrh = 0;
            }
            _jtag_apacc_wr(APRSel.TAR, addr);
            _jtag_apacc_rd(APRSel.DRW);
            return _jtag_dpacc_rd(DPRSel.RDBUFF);
        }
        private void _jtag_apb_wr(Int32 addr, Int32 data)
        {
            if (memo.apsel != APSel.APB_AP || memo.apaddrh != 0)
            {
                _jtag_dpacc_wr(DPRSel.SELECT, APSel.APB_AP.GetHashCode() << 24 | 0 << 4);
                memo.apsel = APSel.APB_AP;
                memo.apaddrh = 0;
            }
            _jtag_apacc_wr(APRSel.TAR, addr);
            _jtag_apacc_wr(APRSel.DRW, data);
        }
        private void _jtag_axi_rd_burst(Int32 addr, Int32 len, Int32 size, Int32[] buff)
        {
            int i;
            if (memo.apsel != APSel.AXI_AP || memo.apaddrh != 0)
            {
                _jtag_dpacc_wr(DPRSel.SELECT, APSel.AXI_AP.GetHashCode() << 24 | 0 << 4);
                memo.apsel = APSel.AXI_AP;
                memo.apaddrh = 0;
            }
            if (memo.axiap_reg.addrinc != 1 || memo.axiap_reg.size != size)
            {
                _jtag_apacc_wr(APRSel.CSW, 1 << 4 | size);
                memo.axiap_reg.addrinc = 1;
                memo.axiap_reg.size = size;
                memo.axiap_reg.sector = 0;
            }
            _jtag_apacc_wr(APRSel.TAR, addr);
            _jtag_apacc_rd(APRSel.DRW);
            for (i = 0; i < len - 1; ++i)
            {
                buff[i] = _jtag_apacc_rd(APRSel.DRW);
            }
            buff[i] = _jtag_dpacc_rd(DPRSel.RDBUFF);
        }
        private void _jtag_axi_wr_burst(Int32 addr, Int32 len, Int32 size, Int32[] buff)
        {
            int i;
            if (memo.apsel != APSel.AXI_AP || memo.apaddrh != 0)
            {
                _jtag_dpacc_wr(DPRSel.SELECT, APSel.AXI_AP.GetHashCode() << 24 | 0 << 4);
                memo.apsel = APSel.AXI_AP;
                memo.apaddrh = 0;
            }
            if (memo.axiap_reg.addrinc != 1 || memo.axiap_reg.size != size || memo.axiap_reg.sector != 0)
            {
                _jtag_apacc_wr(APRSel.CSW, 1 << 4 | size);
                memo.axiap_reg.addrinc = 1;
                memo.axiap_reg.size = size;
                memo.axiap_reg.sector = 0;
            }
            _jtag_apacc_wr(APRSel.TAR, addr);
            for (i = 0; i < len; ++i)
            {
                _jtag_apacc_wr(APRSel.DRW, buff[i]);
            }
        }
        private Int32 _jtag_axi_rd(Int32 addr, Int32 size)
        {
            int i;
            if (memo.apsel != APSel.AXI_AP || memo.apaddrh != 0)
            {
                _jtag_dpacc_wr(DPRSel.SELECT, APSel.AXI_AP.GetHashCode() << 24 | 0 << 4);
                memo.apsel = APSel.AXI_AP;
                memo.apaddrh = 0;
            }
            if (memo.axiap_reg.size != size)
            {
                _jtag_apacc_wr(APRSel.CSW, memo.axiap_reg.addrinc << 4 | size);
                memo.axiap_reg.size = size;
                memo.axiap_reg.sector = 0;
            }
            _jtag_apacc_wr(APRSel.TAR, addr);
            _jtag_apacc_rd(APRSel.DRW);
            return _jtag_dpacc_rd(DPRSel.RDBUFF);
        }
        private void _jtag_axi_wr(Int32 addr, Int32 size, Int32 data)
        {
            int i;
            if (memo.apsel != APSel.AXI_AP || memo.apaddrh != 0)
            {
                _jtag_dpacc_wr(DPRSel.SELECT, APSel.AXI_AP.GetHashCode() << 24 | 0 << 4);
                memo.apsel = APSel.AXI_AP;
                memo.apaddrh = 0;
            }
            if (memo.axiap_reg.size != size)
            {
                _jtag_apacc_wr(APRSel.CSW, memo.axiap_reg.addrinc << 4 | size);
                memo.axiap_reg.size = size;
                memo.axiap_reg.sector = 0;
            }
            _jtag_apacc_wr(APRSel.TAR, addr);
            _jtag_apacc_wr(APRSel.DRW, data);
            _jtag_dpacc_rd(DPRSel.RDBUFF);
        }
        private void _jtag_axi_rd_batch(Int32 addr, Int32 len, Int32[] dbuff, Int32[] rbuff)
        {
            int i;
            if (memo.apsel != APSel.AXI_AP || memo.apaddrh != 0)
            {
                _jtag_dpacc_wr(DPRSel.SELECT, APSel.AXI_AP.GetHashCode() << 24 | 0 << 4);
                memo.apsel = APSel.AXI_AP;
                memo.apaddrh = 0;
            }
            if (memo.axiap_reg.size != 2 || memo.axiap_reg.sector != 1)
            {
                _jtag_apacc_wr(APRSel.CSW, memo.axiap_reg.addrinc << 4 | 1 << 3 | 2);
                memo.axiap_reg.size = 2;
                memo.axiap_reg.sector = 1;
            }
            _jtag_apacc_wr(APRSel.TAR, addr);
            _jtag_apacc_rd(APRSel.DRW);
            _jtag_dpacc_rd(DPRSel.RDBUFF);
            _jtag_apdbuf_rd(len, dbuff);
            _jtag_aprbuf_rd(len, rbuff);
        }
        private void _jtag_axi_wr_batch(Int32 addr, Int32 len, Int32[] buff)
        {
            int i;
            _jtag_apwbuf_wr(len, buff);
            if (memo.apsel != APSel.AXI_AP || memo.apaddrh != 0)
            {
                _jtag_dpacc_wr(DPRSel.SELECT, APSel.AXI_AP.GetHashCode() << 24 | 0 << 4);
                memo.apsel = APSel.AXI_AP;
                memo.apaddrh = 0;
            }
            if (memo.axiap_reg.size != 2 || memo.axiap_reg.sector != 1)
            {
                _jtag_apacc_wr(APRSel.CSW, memo.axiap_reg.addrinc << 4 | 1 << 3 | 2);
                memo.axiap_reg.size = 2;
                memo.axiap_reg.sector = 1;
            }
            _jtag_apacc_wr(APRSel.TAR, addr);
            _jtag_apacc_wr(APRSel.DRW, 0);
            _jtag_dpacc_rd(DPRSel.RDBUFF);
        }
        private void _jtag_reset()
        {
            _jtag_tms(6, 0x1f);
            memo.ir = IRSel.IDCODE;
            memo.apsel = APSel.APB_AP;
            memo.apaddrh = 0;
            memo.apbap_reg.prot = 0;
            memo.apbap_reg.addrinc = 0;
            memo.apbap_reg.size = 2;
            memo.axiap_reg.prot = 0;
            memo.axiap_reg.addrinc = 0;
            memo.axiap_reg.sector = 0;
            memo.axiap_reg.size = 2;

        }
        private void _jtag_test()
        {
            _jtag_tms(8, 0);
            _jtag_tms(8, 0xff);
            _jtag_tms(8, 0);
            _jtag_tms(8, 0xff);
            _send_txbuff();
        }
        private void do_cmd()
        {
            DAPCMDPacket pkg;
            while (do_cmd_thread_run || cmd_queue.Count != 0)
            {
                if (cmd_queue.Count == 0)
                {
                    Thread.Sleep(100);
                }
                else
                {
                    queue_mutex.WaitOne();
                    pkg = cmd_queue.Dequeue();
                    queue_mutex.ReleaseMutex();
                    switch (pkg.cmd)
                    {
                        case DAPCMD.Test:
                            _jtag_test();
                            break;
                        case DAPCMD.Reset:
                            _jtag_reset();
                            break;
                        case DAPCMD.APBRead:
                            pkg.dbuff[0] = _jtag_apb_rd(pkg.addr);
                            break;
                        case DAPCMD.APBWrite:
                            _jtag_apb_wr(pkg.addr, pkg.dbuff[0]);
                            break;
                        case DAPCMD.AXIRead:
                            _jtag_axi_rd_burst(pkg.addr, pkg.len, pkg.size, pkg.dbuff);
                            break;
                        case DAPCMD.AXIWrite:
                            _jtag_axi_wr_burst(pkg.addr, pkg.len, pkg.size, pkg.dbuff);
                            break;
                        case DAPCMD.AXIReadBatch:
                            _jtag_axi_rd_batch(pkg.addr, pkg.len, pkg.dbuff, pkg.rbuff);
                            break;
                        case DAPCMD.AXIWriteBatch:
                            _jtag_axi_wr_batch(pkg.addr, pkg.len, pkg.dbuff);
                            break;
                    }
                    pkg.done.val = true;
                }
            }
        }
        public void cmd_enqueue(DAPCMDPacket pkg)
        {
            queue_mutex.WaitOne();
            cmd_queue.Enqueue(pkg);
            queue_mutex.ReleaseMutex();
        }
    }
}
