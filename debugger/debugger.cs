using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;



namespace debugger
{
    public partial class debugger : Form
    {
        DAP dap;
        #region Send Data Window
        private Form send_data_window;
        private TextBox addr_text;
        private TextBox val_text;
        private Button ok_btn;
        #endregion
        static int DBGAPB_DBG_EN = 0x000;
        static int DBGAPB_INST = 0x004;
        static int DBGAPB_INST_WR = 0x008;
        static int DBGAPB_WDATA_L = 0x010;
        static int DBGAPB_WDATA_H = 0x014;
        static int DBGAPB_WDATA_WR = 0x01C;
        static int DBGAPB_RDATA_L = 0x020;
        static int DBGAPB_RDATA_H = 0x024;
        static int INST_ATTACH = 0x000;
        static int INST_RESUME = 0x001;
        static int INST_INSTREG_WR = 0x002;
        static int INST_EXECUTE = 0x003;
        static int INST_STATUS_RD = 0x004;
        static int INST_PC_RD = 0x005;
        static int INST_GPR_RD = 0x006;
        static int INST_CSR_RD = 0x007;
        static int INST_GPR_WR = 0x008;
        static int INST_CSR_WR = 0x009;

        enum FormState
        {
            IDLE,
            RUNING,
            DOWNLOADING,
        }
        #region DBGAPB info
        Tuple<string, int>[] dbgapb_rdata_cmd = new Tuple<string, int>[] {
            new Tuple<string, int>("STATUS", INST_STATUS_RD),
            new Tuple<string, int>("PC", INST_PC_RD),
            new Tuple<string, int>("GPR_ZERO", 0<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_RA", 1<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_SP", 2<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_GP", 3<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_TP", 4<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_T0", 5<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_T1", 6<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_T2", 7<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_S0", 8<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_S1", 9<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_A0", 10<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_A1", 11<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_A2", 12<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_A3", 13<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_A4", 14<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_A5", 15<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_A6", 16<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_A7", 17<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_S2", 18<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_S3", 19<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_S4", 20<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_S5", 21<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_S6", 22<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_S7", 23<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_S8", 24<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_S9", 25<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_S10", 26<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_S11", 27<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_T3", 28<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_T4", 29<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_T5", 30<<16 | INST_GPR_RD),
            new Tuple<string, int>("GPR_T6", 31<<16 | INST_GPR_RD),
            new Tuple<string, int>("CSR_USTATUS",  0x000 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_UIE",  0x004 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_UTVEC",  0x005 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_USCRATCH",  0x040 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_UEPC",  0x041 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_UCAUSE",  0x042 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_UTVAL",  0x043 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_UIP",  0x044 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_FFLAGS",  0x001 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_FRM",  0x002 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_FCSR",  0x003 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_CYCLE",  0xc00 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_TIME",  0xc01 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_INSTRET",  0xc02 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER3",  0xc03 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER4",  0xc04 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER5",  0xc05 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER6",  0xc06 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER7",  0xc07 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER8",  0xc08 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER9",  0xc09 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER10",  0xc0a << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER11",  0xc0b << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER12",  0xc0c << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER13",  0xc0d << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER14",  0xc0e << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER15",  0xc0f << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER16",  0xc10 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER17",  0xc11 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER18",  0xc12 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER19",  0xc13 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER20",  0xc14 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER21",  0xc15 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER22",  0xc16 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER23",  0xc17 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER24",  0xc18 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER25",  0xc19 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER26",  0xc1a << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER27",  0xc1b << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER28",  0xc1c << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER29",  0xc1d << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER30",  0xc1e << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER31",  0xc1f << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_CYCLEH",  0xc80 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_TIMEH",  0xc81 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_INSTRETH",  0xc82 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER3H",  0xc83 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER4H",  0xc84 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER5H",  0xc85 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER6H",  0xc86 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER7H",  0xc87 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER8H",  0xc88 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER9H",  0xc89 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER10H",  0xc8a << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER11H",  0xc8b << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER12H",  0xc8c << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER13H",  0xc8d << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER14H",  0xc8e << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER15H",  0xc8f << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER16H",  0xc90 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER17H",  0xc91 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER18H",  0xc92 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER19H",  0xc93 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER20H",  0xc94 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER21H",  0xc95 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER22H",  0xc96 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER23H",  0xc97 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER24H",  0xc98 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER25H",  0xc99 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER26H",  0xc9a << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER27H",  0xc9b << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER28H",  0xc9c << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER29H",  0xc9d << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER30H",  0xc9e << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_HPMCOUNTER31H",  0xc9f << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_SSTATUS",  0x100 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_SEDELEG",  0x102 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_SIDELEG",  0x103 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_SIE",  0x104 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_STVEC",  0x105 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_SCOUNTEREN",  0x106 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_SSCRATCH",  0x140 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_SEPC",  0x141 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_SCAUSE",  0x142 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_STVAL",  0x143 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_SIP",  0x144 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_SATP",  0x180 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MVENDORID",  0xf11 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MARCHID",  0xf12 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MIMPID",  0xf13 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHARTID",  0xf14 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MSTATUS",  0x300 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MISA",  0x301 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MEDELEG",  0x302 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MIDELEG",  0x303 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MIE",  0x304 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MTVEC",  0x305 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MCOUNTEREN",  0x306 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MSCRATCH",  0x340 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MEPC",  0x341 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MCAUSE",  0x342 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MTVAL",  0x343 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MIP",  0x344 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPCFG0",  0x3a0 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPCFG1",  0x3a1 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPCFG2",  0x3a2 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPCFG3",  0x3a3 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPADDR0",  0x3b0 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPADDR1",  0x3b1 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPADDR2",  0x3b2 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPADDR3",  0x3b3 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPADDR4",  0x3b4 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPADDR5",  0x3b5 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPADDR6",  0x3b6 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPADDR7",  0x3b7 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPADDR8",  0x3b8 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPADDR9",  0x3b9 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPADDR10",  0x3ba << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPADDR11",  0x3bb << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPADDR12",  0x3bc << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPADDR13",  0x3bd << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPADDR14",  0x3be << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMPADDR15",  0x3bf << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMACFG0",  0x3c0 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMACFG1",  0x3c1 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMACFG2",  0x3c2 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMACFG3",  0x3c3 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMAADDR0",  0x3d0 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMAADDR1",  0x3d1 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMAADDR2",  0x3d2 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMAADDR3",  0x3d3 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMAADDR4",  0x3d4 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMAADDR5",  0x3d5 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMAADDR6",  0x3d6 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMAADDR7",  0x3d7 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMAADDR8",  0x3d8 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMAADDR9",  0x3d9 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMAADDR10",  0x3da << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMAADDR11",  0x3db << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMAADDR12",  0x3dc << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMAADDR13",  0x3dd << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMAADDR14",  0x3de << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_PMAADDR15",  0x3df << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MCYCLE",  0xb00 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MINSTRET",  0xb02 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER3",  0xb03 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER4",  0xb04 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER5",  0xb05 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER6",  0xb06 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER7",  0xb07 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER8",  0xb08 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER9",  0xb09 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER10",  0xb0a << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER11",  0xb0b << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER12",  0xb0c << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER13",  0xb0d << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER14",  0xb0e << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER15",  0xb0f << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER16",  0xb10 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER17",  0xb11 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER18",  0xb12 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER19",  0xb13 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER20",  0xb14 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER21",  0xb15 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER22",  0xb16 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER23",  0xb17 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER24",  0xb18 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER25",  0xb19 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER26",  0xb1a << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER27",  0xb1b << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER28",  0xb1c << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER29",  0xb1d << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER30",  0xb1e << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER31",  0xb1f << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MCYCLEH",  0xb80 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MINSTRETH",  0xb82 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER3H",  0xb83 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER4H",  0xb84 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER5H",  0xb85 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER6H",  0xb86 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER7H",  0xb87 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER8H",  0xb88 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER9H",  0xb89 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER10H", 0xb8a << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER11H", 0xb8b << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER12H", 0xb8c << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER13H", 0xb8d << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER14H", 0xb8e << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER15H", 0xb8f << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER16H", 0xb90 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER17H", 0xb91 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER18H", 0xb92 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER19H", 0xb93 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER20H", 0xb94 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER21H", 0xb95 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER22H", 0xb96 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER23H", 0xb97 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER24H", 0xb98 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER25H", 0xb99 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER26H", 0xb9a << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER27H", 0xb9b << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER28H", 0xb9c << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER29H", 0xb9d << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER30H", 0xb9e << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMCOUNTER31H", 0xb9f << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT3",  0x323 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT4",  0x324 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT5",  0x325 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT6",  0x326 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT7",  0x327 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT8",  0x328 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT9",  0x329 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT10",  0x32a << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT11",  0x32b << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT12",  0x32c << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT13",  0x32d << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT14",  0x32e << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT15",  0x32f << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT16",  0x330 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT17",  0x331 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT18",  0x332 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT19",  0x333 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT20",  0x334 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT21",  0x335 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT22",  0x336 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT23",  0x337 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT24",  0x338 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT25",  0x339 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT26",  0x33a << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT27",  0x33b << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT28",  0x33c << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT29",  0x33d << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT30",  0x33e << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_MHPMEVENT31",  0x33f << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_TSELECT",  0x7a0 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_TDATA1",  0x7a1 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_TDATA2",  0x7a2 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_TDATA3",  0x7a3 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_DCSR",  0x7b0 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_DPC",  0x7b1 << 16 | INST_CSR_RD),
            new Tuple<string, int>("CSR_DSCRATCH",  0x7b2 << 16 | INST_CSR_RD),
        };
        #endregion
        Graphics g;
        Thread upd_val_thread;
        Thread loadcode_thread;
        bool upd_val_thread_run;
        FormState form_state = FormState.IDLE;
        FormState form_state_pre;
        Double download_percent;
        Int32[] mem_val;
        Int32[] mem_resp;
        UInt32 mem_addr;
        bool label_sel;
        Int32 label_sel_id;
        int label_cur_id;
        int label_min_id;
        int label_max_id;
        DAP.DSize data_size;

        #region Data Region
        int top_margin = 5;
        int left_margin = 5;
        int row_space = 18;
        int byte_col_space = 23;
        int hword_col_space = 46;
        int word_col_space = 92;
        int ascii_col_space = 10;
        int addr_label_margin = 70;
        int byte_label_margin = 470;
        int byte_left_margin;
        int byte_top_margin;
        int ascii_left_margin;
        int ascii_top_margin;
        Font font = new Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        Brush str_brush = new SolidBrush(Color.Black);
        Brush sel_blk_brush = new SolidBrush(Color.Aqua);
        #endregion

        public debugger()
        {
            InitializeComponent();
            for (int i = 0; i < dbgapb_rdata_cmd.Length; ++i)
                dbg_info_sel.Items.Insert(i, dbgapb_rdata_cmd[i].Item1);
            addr_txt.KeyDown += addr_txt_KeyDown;
            #region Send Data Window
            send_data_window = new Form();
            send_data_window.Text = "Send Data";
            send_data_window.TopMost = true;
            send_data_window.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            send_data_window.StartPosition = FormStartPosition.CenterScreen;
            send_data_window.MaximizeBox = false;
            send_data_window.MinimizeBox = false;
            send_data_window.KeyDown += send_data_window_keydown;
            send_data_window.FormClosing += send_data_window_closing;
            send_data_window.Size = new Size(180, 200);

            addr_text = new TextBox();
            addr_text.Location = new Point(10, 10);
            addr_text.Enabled = false;
            addr_text.KeyDown += send_data_window_keydown;
            send_data_window.Controls.Add(addr_text);

            val_text = new TextBox();
            val_text.Location = new Point(10, 40);
            val_text.KeyDown += send_data_window_keydown;
            send_data_window.Controls.Add(val_text);

            ok_btn = new Button();
            ok_btn.Location = new Point(10, 70);
            ok_btn.Text = "OK";
            ok_btn.Click += send_data_cmd;
            send_data_window.Controls.Add(ok_btn);
            #endregion

            this.FormClosing += debugger_close;
            upd_val_thread = new Thread(update_value);
            upd_val_thread_run = true;
            dap = new DAP();
            mem_val = new Int32[0x40];
            mem_resp = new Int32[0x4] { -1, -1, -1, -1 };
            byte_left_margin = addr_label_margin + left_margin;
            byte_top_margin = row_space + top_margin;
            ascii_left_margin = byte_label_margin + left_margin;
            ascii_top_margin = row_space + top_margin;
            hword_col_space = byte_col_space * 2;
            word_col_space = byte_col_space * 4;

            vScrollBar1.Maximum = 0xffffff0 + 9;
            vScrollBar1.Minimum = 0;

            data_panel.MouseWheel += scrollbar_wheel;
            data_panel.MouseDown += data_region_mouse_down;
            data_panel.MouseUp += data_region_mouse_up;
            data_panel.MouseMove += data_region_mouse_move;
            data_panel.MouseDoubleClick += data_region_mouse_double_click;

            data_size = DAP.DSize.BYTE;
            hword_mode_btn.Enabled = true;
            word_mode_btn.Enabled = true;
        }

        private void debugger_close(object sender, EventArgs e)
        {
            upd_val_thread_run = false;
            dap.close();
        }

        private void setup_btn_Click(object sender, EventArgs e)
        {
            dap.run();
            if (form_state == FormState.IDLE)
            {
                upd_val_thread.Start();
                goto_btn.Enabled = true;
                setup_btn.Enabled = false;
                form_state = FormState.RUNING;
            }
        }
        private void update_value()
        {
            DAP.DAPCMDPacket pkg;
            pkg.done = new DAP.ref_bool();

            while (upd_val_thread_run)
            {
                Thread.Sleep(500);
                pkg.cmd = DAP.DAPCMD.AXIReadBatch;
                pkg.addr = (Int32)mem_addr;
                pkg.size = 2;
                pkg.len = 0x40;
                pkg.dbuff = mem_val;
                pkg.rbuff = mem_resp;
                pkg.done.val = false;
                dap.cmd_enqueue(pkg);
                while (!pkg.done.val && upd_val_thread_run) Thread.Sleep(100);
            }
        }
        private void write_value(Int32 addr, Int32 val, DAP.DSize size = DAP.DSize.WORD)
        {
            if (size > DAP.DSize.WORD)
            {
                MessageBox.Show("Size is larger than WORD");
            }
            DAP.DAPCMDPacket pkg;
            pkg.done = new DAP.ref_bool();

            pkg.cmd = DAP.DAPCMD.AXIWrite;
            pkg.addr = addr;
            pkg.size = size.GetHashCode();
            pkg.len = 0x1;
            pkg.dbuff = new Int32[1];
            pkg.rbuff = new Int32[0];
            pkg.dbuff[0] = val;
            pkg.done.val = false;
            dap.cmd_enqueue(pkg);

        }
        private void goto_btn_Click(object sender, EventArgs e)
        {
            try
            {
                mem_addr = (UInt32)(Int32.Parse(addr_txt.Text, System.Globalization.NumberStyles.HexNumber) & ~0xf);
                vScrollBar1.Value = (Int32)(mem_addr / 0x10);

            }
            catch { }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            refresh_data_region();
            refresh_state();
        }

        private void refresh_data_region()
        {
            DAP.DSize size;
            Int32 resp, data;
            Bitmap bmp = new Bitmap(data_panel.Size.Width, data_panel.Size.Height);
            size = data_size;
            g = Graphics.FromImage(bmp);
            g.FillRectangle(Brushes.White, 0, 0, data_panel.Size.Width, data_panel.Size.Height);
            switch (size)
            {
                case DAP.DSize.BYTE:
                    for (int j = 0; j < 0x10; ++j)
                        g.DrawString(j.ToString("X1"), font, str_brush, j * byte_col_space + addr_label_margin + left_margin + 6, top_margin);
                    break;
                case DAP.DSize.HWORD:
                    for (int j = 0; j < 0x8; ++j)
                        g.DrawString((j * 2).ToString("X1"), font, str_brush, j * hword_col_space + addr_label_margin + left_margin + 17, top_margin);
                    break;
                case DAP.DSize.WORD:
                    for (int j = 0; j < 0x4; ++j)
                        g.DrawString((j * 4).ToString("X1"), font, str_brush, j * word_col_space + addr_label_margin + left_margin + 42, top_margin);
                    break;
            }
            for (int i = 0; i < 0x10; ++i)
            {
                g.DrawString((i * 0x10 + (mem_addr & ~0xf)).ToString("X8"), font, str_brush, left_margin, (i + 1) * row_space + top_margin);
                for (int j = 0; j < 0x10 / 4; ++j)
                {
                    resp = (byte)((mem_resp[(i * 4 + j) / 16] >> (((i * 4 + j) & 0xf) * 2)) & 0x3);
                    switch (size)
                    {
                        case DAP.DSize.BYTE:
                            #region Byte and ASCII
                            switch (resp)
                            {
                                case 0:
                                case 1:
                                    for (int k = 0; k < 4; ++k)
                                    {
                                        if (label_sel && (i * 0x10 + j * 4 + k) >= label_min_id && (i * 0x10 + j * 4 + k) <= label_max_id)
                                        {
                                            g.FillRectangle(sel_blk_brush, (j * 4 + k) * byte_col_space + byte_left_margin, i * row_space + byte_top_margin, byte_col_space, row_space);
                                            g.FillRectangle(sel_blk_brush, (j * 4 + k) * ascii_col_space + ascii_left_margin, i * row_space + ascii_top_margin, ascii_col_space, row_space);
                                        }
                                        data = (byte)(mem_val[i * 4 + j] >> k * 8 & 0xff);
                                        g.DrawString(data.ToString("X2"), font, str_brush, (j * 4 + k) * byte_col_space + addr_label_margin + left_margin + 3, (i + 1) * row_space + top_margin);
                                        g.DrawString(Convert.ToChar(data >= 0x20 && data < 0x7f ? data : 0x2e).ToString(), font, str_brush,
                                                     (j * 4 + k) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                    }
                                    break;
                                case 2:
                                    for (int k = 0; k < 4; ++k)
                                    {
                                        if (label_sel && (i * 0x10 + j * 4 + k) >= label_min_id && (i * 0x10 + j * 4 + k) <= label_max_id)
                                        {
                                            g.FillRectangle(sel_blk_brush, (j * 4 + k) * byte_col_space + byte_left_margin, i * row_space + byte_top_margin, byte_col_space, row_space);
                                            g.FillRectangle(sel_blk_brush, (j * 4 + k) * ascii_col_space + ascii_left_margin, i * row_space + ascii_top_margin, ascii_col_space, row_space);
                                        }
                                        g.DrawString("xx", font, str_brush, (j * 4 + k) * byte_col_space + addr_label_margin + left_margin + 3, (i + 1) * row_space + top_margin);
                                        g.DrawString("x", font, str_brush, (j * 4 + k) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                    }
                                    break;
                                case 3:
                                    for (int k = 0; k < 4; ++k)
                                    {
                                        if (label_sel && (i * 0x10 + j * 4 + k) >= label_min_id && (i * 0x10 + j * 4 + k) <= label_max_id)
                                        {
                                            g.FillRectangle(sel_blk_brush, (j * 4 + k) * byte_col_space + byte_left_margin, i * row_space + byte_top_margin, byte_col_space, row_space);
                                            g.FillRectangle(sel_blk_brush, (j * 4 + k) * ascii_col_space + ascii_left_margin, i * row_space + ascii_top_margin, ascii_col_space, row_space);
                                        }
                                        g.DrawString("--", font, str_brush, (j * 4 + k) * byte_col_space + addr_label_margin + left_margin + 3, (i + 1) * row_space + top_margin);
                                        g.DrawString("-", font, str_brush, (j * 4 + k) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                    }
                                    break;
                            }
                            #endregion
                            break;
                        case DAP.DSize.HWORD:
                            #region Hword and ASCII
                            switch (resp)
                            {
                                case 0:
                                case 1:
                                    for (int k = 0; k < 2; ++k)
                                    {
                                        if (label_sel && (i * 0x10 + j * 4 + k * 2) >= label_min_id && (i * 0x10 + j * 4 + k * 2) <= label_max_id)
                                        {
                                            g.FillRectangle(sel_blk_brush, (j * 2 + k) * hword_col_space + byte_left_margin, i * row_space + byte_top_margin, hword_col_space, row_space);
                                            g.FillRectangle(sel_blk_brush, (j * 4 + k * 2) * ascii_col_space + ascii_left_margin, i * row_space + ascii_top_margin, ascii_col_space * 2, row_space);
                                        }
                                        data = (UInt16)(mem_val[i * 4 + j] >> k * 16 & 0xffff);
                                        g.DrawString(data.ToString("X4"), font, str_brush, (j * 2 + k) * hword_col_space + addr_label_margin + left_margin + 8, (i + 1) * row_space + top_margin);
                                        g.DrawString(Convert.ToChar((data & 0xff) >= 0x20 && (data & 0xff) < 0x7f ? (data & 0xff) : 0x2e).ToString(), font, str_brush,
                                                     (j * 4 + k * 2 + 0) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                        g.DrawString(Convert.ToChar((data >> 8) >= 0x20 && (data >> 8) < 0x7f ? (data >> 8) : 0x2e).ToString(), font, str_brush,
                                                     (j * 4 + k * 2 + 1) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                    }
                                    break;
                                case 2:
                                    for (int k = 0; k < 2; ++k)
                                    {
                                        if (label_sel && (i * 0x10 + j * 4 + k * 2) >= label_min_id && (i * 0x10 + j * 4 + k * 2) <= label_max_id)
                                        {
                                            g.FillRectangle(sel_blk_brush, (j * 2 + k) * hword_col_space + byte_left_margin, i * row_space + byte_top_margin, hword_col_space, row_space);
                                            g.FillRectangle(sel_blk_brush, (j * 4 + k * 2) * ascii_col_space + ascii_left_margin, i * row_space + ascii_top_margin, ascii_col_space * 2, row_space);
                                        }
                                        g.DrawString("xxxx", font, str_brush, (j * 2 + k) * hword_col_space + addr_label_margin + left_margin + 8, (i + 1) * row_space + top_margin);
                                        g.DrawString("x", font, str_brush, (j * 4 + k * 2 + 0) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                        g.DrawString("x", font, str_brush, (j * 4 + k * 2 + 1) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                    }
                                    break;
                                case 3:
                                    for (int k = 0; k < 2; ++k)
                                    {
                                        if (label_sel && (i * 0x10 + j * 4 + k * 2) >= label_min_id && (i * 0x10 + j * 4 + k * 2) <= label_max_id)
                                        {
                                            g.FillRectangle(sel_blk_brush, (j * 2 + k) * hword_col_space + byte_left_margin, i * row_space + byte_top_margin, hword_col_space, row_space);
                                            g.FillRectangle(sel_blk_brush, (j * 4 + k * 2) * ascii_col_space + ascii_left_margin, i * row_space + ascii_top_margin, ascii_col_space * 2, row_space);
                                        }
                                        g.DrawString("----", font, str_brush, (j * 2 + k) * hword_col_space + addr_label_margin + left_margin + 8, (i + 1) * row_space + top_margin);
                                        g.DrawString("-", font, str_brush, (j * 4 + k * 2 + 0) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                        g.DrawString("-", font, str_brush, (j * 4 + k * 2 + 1) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                    }
                                    break;
                            }
                            #endregion
                            break;
                        case DAP.DSize.WORD:
                            #region Word and ASCII
                            switch (resp)
                            {
                                case 0:
                                case 1:
                                    if (label_sel && (i * 0x10 + j * 4) >= label_min_id && (i * 0x10 + j * 4) <= label_max_id)
                                    {
                                        g.FillRectangle(sel_blk_brush, (j) * word_col_space + byte_left_margin, i * row_space + byte_top_margin, word_col_space, row_space);
                                        g.FillRectangle(sel_blk_brush, (j * 4) * ascii_col_space + ascii_left_margin, i * row_space + ascii_top_margin, ascii_col_space * 4, row_space);
                                    }
                                    data = mem_val[i * 4 + j];
                                    g.DrawString(data.ToString("X8"), font, str_brush, (j) * word_col_space + addr_label_margin + left_margin + 20, (i + 1) * row_space + top_margin);
                                    g.DrawString(Convert.ToChar((data & 0xff) >= 0x20 && (data & 0xff) < 0x7f ? (data & 0xff) : 0x2e).ToString(), font, str_brush,
                                                 (j * 4 + 0) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                    g.DrawString(Convert.ToChar((data >> 8 & 0xff) >= 0x20 && (data >> 8 & 0xff) < 0x7f ? (data >> 8 & 0xff) : 0x2e).ToString(), font, str_brush,
                                                 (j * 4 + 1) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                    g.DrawString(Convert.ToChar((data >> 16 & 0xff) >= 0x20 && (data >> 16 & 0xff) < 0x7f ? (data >> 16 & 0xff) : 0x2e).ToString(), font, str_brush,
                                                 (j * 4 + 2) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                    g.DrawString(Convert.ToChar((data >> 24 & 0xff) >= 0x20 && (data >> 24 & 0xff) < 0x7f ? (data >> 24 & 0xff) : 0x2e).ToString(), font, str_brush,
                                                 (j * 4 + 3) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                    break;
                                case 2:
                                    if (label_sel && (i * 0x10 + j * 4) >= label_min_id && (i * 0x10 + j * 4) <= label_max_id)
                                    {
                                        g.FillRectangle(sel_blk_brush, (j) * word_col_space + byte_left_margin, i * row_space + byte_top_margin, word_col_space, row_space);
                                        g.FillRectangle(sel_blk_brush, (j * 4) * ascii_col_space + ascii_left_margin, i * row_space + ascii_top_margin, ascii_col_space * 4, row_space);
                                    }
                                    g.DrawString("xxxxxxxx", font, str_brush, (j) * word_col_space + addr_label_margin + left_margin + 20, (i + 1) * row_space + top_margin);
                                    g.DrawString("x", font, str_brush, (j * 4 + 0) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                    g.DrawString("x", font, str_brush, (j * 4 + 1) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                    g.DrawString("x", font, str_brush, (j * 4 + 2) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                    g.DrawString("x", font, str_brush, (j * 4 + 3) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                    break;
                                case 3:
                                    if (label_sel && (i * 0x10 + j * 4) >= label_min_id && (i * 0x10 + j * 4) <= label_max_id)
                                    {
                                        g.FillRectangle(sel_blk_brush, (j) * word_col_space + byte_left_margin, i * row_space + byte_top_margin, word_col_space, row_space);
                                        g.FillRectangle(sel_blk_brush, (j * 4) * ascii_col_space + ascii_left_margin, i * row_space + ascii_top_margin, ascii_col_space * 4, row_space);
                                    }
                                    g.DrawString("--------", font, str_brush, (j) * word_col_space + addr_label_margin + left_margin + 18, (i + 1) * row_space + top_margin);
                                    g.DrawString("-", font, str_brush, (j * 4 + 0) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                    g.DrawString("-", font, str_brush, (j * 4 + 1) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                    g.DrawString("-", font, str_brush, (j * 4 + 2) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin);
                                    g.DrawString("-", font, str_brush, (j * 4 + 3) * ascii_col_space + byte_label_margin + left_margin, (i + 1) * row_space + top_margin); break;
                            }
                            #endregion
                            break;
                    }
                }
            }
            data_panel.CreateGraphics().DrawImage(bmp, new PointF(0, 0));
        }
        private void refresh_state()
        {
            switch (form_state)
            {
                case FormState.IDLE:
                    formstate_lbl.Text = "Need to setup";
                    formstate_lbl.BackColor = Color.LightGray;
                    break;
                case FormState.RUNING:
                    formstate_lbl.Text = "Active";
                    formstate_lbl.BackColor = Color.LightGreen;
                    if (form_state_pre == FormState.DOWNLOADING)
                        dwnld_btn.Enabled = true;
                    break;
                case FormState.DOWNLOADING:
                    formstate_lbl.Text = string.Format("Downloading: {0:P1}", download_percent);
                    formstate_lbl.BackColor = Color.LightPink;
                    break;
            }
            form_state_pre = form_state;
        }
        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            mem_addr = (UInt32)e.NewValue * 0x10;
        }
        private void data_region_mouse_double_click(object sender, MouseEventArgs e)
        {
            int id;
            if (label_sel)
            {
                addr_text.Text = (mem_addr + label_sel_id).ToString("X8");
                switch (data_size)
                {
                    case DAP.DSize.BYTE:
                        val_text.Text = ((mem_val[label_sel_id / 4] >> 8 * (label_sel_id & 0x3)) & 0xff).ToString("X2");
                        break;
                    case DAP.DSize.HWORD:
                        val_text.Text = ((mem_val[label_sel_id / 4] >> 8 * (label_sel_id & 0x2)) & 0xffff).ToString("X4");
                        break;
                    case DAP.DSize.WORD:
                        val_text.Text = mem_val[label_sel_id / 4].ToString("X8");
                        break;
                }
                send_data_window.Visible = true;
                val_text.Focus();
                val_text.Select(0, val_text.Text.Length);
            }

        }
        private void data_region_mouse_down(object sender, MouseEventArgs e)
        {
            Int32 byte_top, byte_left;
            byte_left = e.X - byte_left_margin;
            byte_top = e.Y - byte_top_margin;
            if (byte_left >= 0 && byte_top >= 0 && byte_left < 0x10 * byte_col_space && byte_top < 0x10 * row_space)
            {
                label_sel = true;
                label_min_id = label_max_id = label_cur_id = label_sel_id = (byte_top / row_space * 0x10 + byte_left / byte_col_space) & -(1 << data_size.GetHashCode());
                refresh_data_region();
            }
        }
        private void data_region_mouse_up(object sender, MouseEventArgs e)
        {
            if (label_sel)
            {
                label_sel = false;
                label_min_id = -1;
                label_max_id = -1;
                refresh_data_region();
            }
        }
        private void data_region_mouse_move(object sender, MouseEventArgs e)
        {
            Int32 byte_top, byte_left;
            byte_left = e.X - byte_left_margin;
            byte_top = e.Y - byte_top_margin;
            if (label_sel && byte_left >= 0 && byte_top >= 0 && byte_left < 0x10 * byte_col_space && byte_top < 0x10 * row_space)
            {
                label_cur_id = (byte_top / row_space * 0x10 + byte_left / byte_col_space) & -(1 << data_size.GetHashCode());
                label_min_id = label_cur_id < label_sel_id ? label_cur_id : label_sel_id;
                label_max_id = label_cur_id > label_sel_id ? label_cur_id : label_sel_id;
            }
        }
        private void scrollbar_wheel(object sender, MouseEventArgs e)
        {
            Int64 val = (Int64)vScrollBar1.Value * 0x10 - ((Int64)e.Delta & ~0xf);
            mem_addr = val < 0 ? 0 :
                       val >= 0x100000000 - 0x100 ? 0xffffff00 : (UInt32)val;
            vScrollBar1.Value = (Int32)(mem_addr / 0x10);

        }
        private void send_data_window_closing(object sender, CancelEventArgs e)
        {
            ((Form)sender).Visible = false;
            e.Cancel = true;
        }
        private void send_data_window_keydown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    send_data_window.Visible = false;
                    break;
                case Keys.Enter:
                    send_data_cmd(sender, e);
                    break;
            }
        }
        private void send_data_cmd(object sender, EventArgs e)
        {
            send_data_window.Visible = false;
            try
            {
                write_value(Int32.Parse(addr_text.Text, System.Globalization.NumberStyles.HexNumber),
                            Int32.Parse(val_text.Text, System.Globalization.NumberStyles.HexNumber),
                            data_size);
            }
            catch { }
        }
        private void byte_mode_btn_Click(object sender, EventArgs e)
        {
            data_size = DAP.DSize.BYTE;
            byte_mode_btn.Enabled = false;
            hword_mode_btn.Enabled = true;
            word_mode_btn.Enabled = true;
        }
        private void hword_mode_btn_Click(object sender, EventArgs e)
        {
            data_size = DAP.DSize.HWORD;
            byte_mode_btn.Enabled = true;
            hword_mode_btn.Enabled = false;
            word_mode_btn.Enabled = true;
        }
        private void word_mode_btn_Click(object sender, EventArgs e)
        {
            data_size = DAP.DSize.WORD;
            byte_mode_btn.Enabled = true;
            hword_mode_btn.Enabled = true;
            word_mode_btn.Enabled = false;
        }
        private void rst_btn_Click(object sender, EventArgs e)
        {
            DAP.DAPCMDPacket pkg;
            pkg.done = new DAP.ref_bool();

            pkg.cmd = DAP.DAPCMD.Reset;
            pkg.addr = 0;
            pkg.size = 0;
            pkg.len = 0x1;
            pkg.dbuff = new Int32[0];
            pkg.rbuff = new Int32[0];
            pkg.done.val = false;
            dap.cmd_enqueue(pkg);
        }
        private void addr_txt_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    goto_btn.Focus();
                    goto_btn_Click(sender, null);
                    break;
            }
        }
        private void download_romcode(object _filepath)
        {
            string filepath = (string)_filepath;
            int addr = 0, val;
            long len, pos;
            DAP.DAPCMDPacket pkg;
            pkg.done = new DAP.ref_bool();
            pkg.dbuff = new Int32[0x40];
            pkg.rbuff = new Int32[0];

            using (var stream = File.Open(filepath, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream, Encoding.ASCII, false))
                {
                    while ((pos = stream.Position) < (len = stream.Length))
                    {
                        pkg.addr = addr;
                        if (len - pos >= 0x40 * 4)
                        {
                            pkg.cmd = DAP.DAPCMD.AXIWriteBatch;
                            pkg.size = 2;
                            pkg.len = 0x40;
                            for (int i = 0; i < 0x40; ++i)
                                pkg.dbuff[i] = reader.ReadInt32();
                        }
                        else if (len - pos >= 4)
                        {
                            pkg.cmd = DAP.DAPCMD.AXIWrite;
                            pkg.size = 2;
                            pkg.len = (int)(stream.Length - stream.Position) / 4;
                            for (int i = 0; i < pkg.len; ++i)
                                pkg.dbuff[i] = reader.ReadInt32();
                        }
                        else if (len - pos >= 2)
                        {
                            pkg.cmd = DAP.DAPCMD.AXIWrite;
                            pkg.size = 1;
                            pkg.len = 0x1;
                            pkg.dbuff[0] = reader.ReadInt16();
                        }
                        else
                        {
                            pkg.cmd = DAP.DAPCMD.AXIWrite;
                            pkg.size = 0;
                            pkg.len = 0x1;
                            pkg.dbuff[0] = reader.ReadByte();
                        }
                        pkg.done.val = false;
                        dap.cmd_enqueue(pkg);
                        while (!pkg.done.val) Thread.Sleep(100);
                        addr += pkg.len << pkg.size;
                        download_percent = stream.Position * 1.0 / stream.Length;
                    }
                    MessageBox.Show("Download ROM code done");
                }
            }
            form_state = FormState.RUNING;
        }
        private void dwnld_btn_Click(object sender, EventArgs e)
        {
            string filepath = "";

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filepath = openFileDialog.FileName;
                    loadcode_thread = new Thread(new ParameterizedThreadStart(download_romcode));
                    loadcode_thread.Start(filepath);
                    dwnld_btn.Enabled = false;
                    form_state = FormState.DOWNLOADING;
                }
            }

        }
        private void dbgapb_req_btn_Click(object sender, EventArgs e)
        {
            DAP.DAPCMDPacket pkg;
            pkg.done = new DAP.ref_bool();
            pkg.dbuff = new Int32[1];
            pkg.rbuff = new Int32[0];
            pkg.size = 2;
            pkg.len = 0x1;
            Int64 apb_rdata = 0;

            // debug enable
            pkg.cmd = DAP.DAPCMD.APBWrite;
            pkg.addr = DBGAPB_DBG_EN;
            pkg.dbuff[0] = 1;
            pkg.done.val = false;
            dap.cmd_enqueue(pkg);
            while (!pkg.done.val) Thread.Sleep(100);

            // attach enable
            pkg.cmd = DAP.DAPCMD.APBWrite;
            pkg.addr = DBGAPB_INST;
            pkg.dbuff[0] = INST_ATTACH;
            pkg.done.val = false;
            dap.cmd_enqueue(pkg);
            while (!pkg.done.val) Thread.Sleep(100);
            pkg.addr = DBGAPB_INST_WR;
            pkg.dbuff[0] = 1;
            pkg.done.val = false;
            dap.cmd_enqueue(pkg);
            while (!pkg.done.val) Thread.Sleep(100);

            // read dbg info
            pkg.cmd = DAP.DAPCMD.APBWrite;
            pkg.addr = DBGAPB_INST;
            pkg.dbuff[0] = dbgapb_rdata_cmd[dbg_info_sel.SelectedIndex].Item2;
            pkg.done.val = false;
            dap.cmd_enqueue(pkg);
            while (!pkg.done.val) Thread.Sleep(100);
            pkg.addr = DBGAPB_INST_WR;
            pkg.dbuff[0] = 1;
            pkg.done.val = false;
            dap.cmd_enqueue(pkg);
            while (!pkg.done.val) Thread.Sleep(100);
            pkg.cmd = DAP.DAPCMD.APBRead;
            pkg.addr = DBGAPB_RDATA_H;
            pkg.dbuff[0] = 0;
            pkg.done.val = false;
            dap.cmd_enqueue(pkg);
            while (!pkg.done.val) Thread.Sleep(100);
            apb_rdata = (Int64)(pkg.dbuff[0]) << 32;
            pkg.cmd = DAP.DAPCMD.APBRead;
            pkg.addr = DBGAPB_RDATA_L;
            pkg.dbuff[0] = 0;
            pkg.done.val = false;
            dap.cmd_enqueue(pkg);
            while (!pkg.done.val) Thread.Sleep(100);
            apb_rdata |= pkg.dbuff[0];

            // resume
            pkg.cmd = DAP.DAPCMD.APBWrite;
            pkg.addr = DBGAPB_INST;
            pkg.dbuff[0] = INST_RESUME;
            pkg.done.val = false;
            dap.cmd_enqueue(pkg);
            while (!pkg.done.val) Thread.Sleep(100);
            pkg.addr = DBGAPB_INST_WR;
            pkg.dbuff[0] = 1;
            pkg.done.val = false;
            dap.cmd_enqueue(pkg);
            while (!pkg.done.val) Thread.Sleep(100);

            MessageBox.Show(string.Format("{0}: {1:X16}", dbgapb_rdata_cmd[dbg_info_sel.SelectedIndex].Item1, apb_rdata));
        }
        private string regs_name(Int16 reg_id) {
            switch (reg_id) {
                case 0: return "zero";
                case 1: return "ra";
                case 2: return "sp";
                case 3: return "gp";
                case 4: return "tp";
                case 5: return "t0";
                case 6: return "t1";
                case 7: return "t2";
                case 8: return "s0";
                case 9: return "s1";
                case 10: return "a0";
                case 11: return "a1";
                case 12: return "a2";
                case 13: return "a3";
                case 14: return "a4";
                case 15: return "a5";
                case 16: return "a6";
                case 17: return "a7";
                case 18: return "s2";
                case 19: return "s3";
                case 20: return "s4";
                case 21: return "s5";
                case 22: return "s6";
                case 23: return "s7";
                case 24: return "s8";
                case 25: return "s9";
                case 26: return "s10";
                case 27: return "s11";
                case 28: return "t3";
                case 29: return "t4";
                case 30: return "t5";
                case 31: return "t6";
                default: return "unknown reg";
            }
        }
        private string csr_name(Int16 csr_addr) {
            switch (csr_addr)
            {
                case 0x000: return "ustatus";
                case 0x004: return "uie";
                case 0x005: return "utvec";
                case 0x040: return "uscratch";
                case 0x041: return "uepc";
                case 0x042: return "ucause";
                case 0x043: return "utval";
                case 0x044: return "uip";
                case 0x001: return "fflags";
                case 0x002: return "frm";
                case 0x003: return "fcsr";
                case 0xc00: return "cycle";
                case 0xc01: return "time";
                case 0xc02: return "instret";
                case 0xc03: return "hpmcounter3";
                case 0xc04: return "hpmcounter4";
                case 0xc05: return "hpmcounter5";
                case 0xc06: return "hpmcounter6";
                case 0xc07: return "hpmcounter7";
                case 0xc08: return "hpmcounter8";
                case 0xc09: return "hpmcounter9";
                case 0xc0a: return "hpmcounter10";
                case 0xc0b: return "hpmcounter11";
                case 0xc0c: return "hpmcounter12";
                case 0xc0d: return "hpmcounter13";
                case 0xc0e: return "hpmcounter14";
                case 0xc0f: return "hpmcounter15";
                case 0xc10: return "hpmcounter16";
                case 0xc11: return "hpmcounter17";
                case 0xc12: return "hpmcounter18";
                case 0xc13: return "hpmcounter19";
                case 0xc14: return "hpmcounter20";
                case 0xc15: return "hpmcounter21";
                case 0xc16: return "hpmcounter22";
                case 0xc17: return "hpmcounter23";
                case 0xc18: return "hpmcounter24";
                case 0xc19: return "hpmcounter25";
                case 0xc1a: return "hpmcounter26";
                case 0xc1b: return "hpmcounter27";
                case 0xc1c: return "hpmcounter28";
                case 0xc1d: return "hpmcounter29";
                case 0xc1e: return "hpmcounter30";
                case 0xc1f: return "hpmcounter31";
                case 0xc80: return "cycleh";
                case 0xc81: return "timeh";
                case 0xc82: return "instreth";
                case 0xc83: return "hpmcounter3h";
                case 0xc84: return "hpmcounter4h";
                case 0xc85: return "hpmcounter5h";
                case 0xc86: return "hpmcounter6h";
                case 0xc87: return "hpmcounter7h";
                case 0xc88: return "hpmcounter8h";
                case 0xc89: return "hpmcounter9h";
                case 0xc8a: return "hpmcounter10h";
                case 0xc8b: return "hpmcounter11h";
                case 0xc8c: return "hpmcounter12h";
                case 0xc8d: return "hpmcounter13h";
                case 0xc8e: return "hpmcounter14h";
                case 0xc8f: return "hpmcounter15h";
                case 0xc90: return "hpmcounter16h";
                case 0xc91: return "hpmcounter17h";
                case 0xc92: return "hpmcounter18h";
                case 0xc93: return "hpmcounter19h";
                case 0xc94: return "hpmcounter20h";
                case 0xc95: return "hpmcounter21h";
                case 0xc96: return "hpmcounter22h";
                case 0xc97: return "hpmcounter23h";
                case 0xc98: return "hpmcounter24h";
                case 0xc99: return "hpmcounter25h";
                case 0xc9a: return "hpmcounter26h";
                case 0xc9b: return "hpmcounter27h";
                case 0xc9c: return "hpmcounter28h";
                case 0xc9d: return "hpmcounter29h";
                case 0xc9e: return "hpmcounter30h";
                case 0xc9f: return "hpmcounter31h";

                case 0x100: return "sstatus";
                case 0x102: return "sedeleg";
                case 0x103: return "sideleg";
                case 0x104: return "sie";
                case 0x105: return "stvec";
                case 0x106: return "scounteren";
                case 0x140: return "sscratch";
                case 0x141: return "sepc";
                case 0x142: return "scause";
                case 0x143: return "stval";
                case 0x144: return "sip";
                case 0x180: return "satp";
                case 0xf11: return "mvendorid";
                case 0xf12: return "marchid";
                case 0xf13: return "mimpid";
                case 0xf14: return "mhartid";
                case 0x300: return "mstatus";
                case 0x301: return "misa";
                case 0x302: return "medeleg";
                case 0x303: return "mideleg";
                case 0x304: return "mie";
                case 0x305: return "mtvec";
                case 0x306: return "mcounteren";
                case 0x340: return "mscratch";
                case 0x341: return "mepc";
                case 0x342: return "mcause";
                case 0x343: return "mtval";
                case 0x344: return "mip";
                case 0x3a0: return "pmpcfg0";
                case 0x3a1: return "pmpcfg1";
                case 0x3a2: return "pmpcfg2";
                case 0x3a3: return "pmpcfg3";
                case 0x3b0: return "pmpaddr0";
                case 0x3b1: return "pmpaddr1";
                case 0x3b2: return "pmpaddr2";
                case 0x3b3: return "pmpaddr3";
                case 0x3b4: return "pmpaddr4";
                case 0x3b5: return "pmpaddr5";
                case 0x3b6: return "pmpaddr6";
                case 0x3b7: return "pmpaddr7";
                case 0x3b8: return "pmpaddr8";
                case 0x3b9: return "pmpaddr9";
                case 0x3ba: return "pmpaddr10";
                case 0x3bb: return "pmpaddr11";
                case 0x3bc: return "pmpaddr12";
                case 0x3bd: return "pmpaddr13";
                case 0x3be: return "pmpaddr14";
                case 0x3bf: return "pmpaddr15";
                case 0x3c0: return "pmacfg0";
                case 0x3c1: return "pmacfg1";
                case 0x3c2: return "pmacfg2";
                case 0x3c3: return "pmacfg3";
                case 0x3d0: return "pmaaddr0";
                case 0x3d1: return "pmaaddr1";
                case 0x3d2: return "pmaaddr2";
                case 0x3d3: return "pmaaddr3";
                case 0x3d4: return "pmaaddr4";
                case 0x3d5: return "pmaaddr5";
                case 0x3d6: return "pmaaddr6";
                case 0x3d7: return "pmaaddr7";
                case 0x3d8: return "pmaaddr8";
                case 0x3d9: return "pmaaddr9";
                case 0x3da: return "pmaaddr10";
                case 0x3db: return "pmaaddr11";
                case 0x3dc: return "pmaaddr12";
                case 0x3dd: return "pmaaddr13";
                case 0x3de: return "pmaaddr14";
                case 0x3df: return "pmaaddr15";
                case 0xb00: return "mcycle";
                case 0xb02: return "minstret";
                case 0xb03: return "mhpmcounter3";
                case 0xb04: return "mhpmcounter4";
                case 0xb05: return "mhpmcounter5";
                case 0xb06: return "mhpmcounter6";
                case 0xb07: return "mhpmcounter7";
                case 0xb08: return "mhpmcounter8";
                case 0xb09: return "mhpmcounter9";
                case 0xb0a: return "mhpmcounter10";
                case 0xb0b: return "mhpmcounter11";
                case 0xb0c: return "mhpmcounter12";
                case 0xb0d: return "mhpmcounter13";
                case 0xb0e: return "mhpmcounter14";
                case 0xb0f: return "mhpmcounter15";
                case 0xb10: return "mhpmcounter16";
                case 0xb11: return "mhpmcounter17";
                case 0xb12: return "mhpmcounter18";
                case 0xb13: return "mhpmcounter19";
                case 0xb14: return "mhpmcounter20";
                case 0xb15: return "mhpmcounter21";
                case 0xb16: return "mhpmcounter22";
                case 0xb17: return "mhpmcounter23";
                case 0xb18: return "mhpmcounter24";
                case 0xb19: return "mhpmcounter25";
                case 0xb1a: return "mhpmcounter26";
                case 0xb1b: return "mhpmcounter27";
                case 0xb1c: return "mhpmcounter28";
                case 0xb1d: return "mhpmcounter29";
                case 0xb1e: return "mhpmcounter30";
                case 0xb1f: return "mhpmcounter31";
                case 0xb80: return "mcycleh";
                case 0xb82: return "minstreth";
                case 0xb83: return "mhpmcounter3h";
                case 0xb84: return "mhpmcounter4h";
                case 0xb85: return "mhpmcounter5h";
                case 0xb86: return "mhpmcounter6h";
                case 0xb87: return "mhpmcounter7h";
                case 0xb88: return "mhpmcounter8h";
                case 0xb89: return "mhpmcounter9h";
                case 0xb8a: return "mhpmcounter10h";
                case 0xb8b: return "mhpmcounter11h";
                case 0xb8c: return "mhpmcounter12h";
                case 0xb8d: return "mhpmcounter13h";
                case 0xb8e: return "mhpmcounter14h";
                case 0xb8f: return "mhpmcounter15h";
                case 0xb90: return "mhpmcounter16h";
                case 0xb91: return "mhpmcounter17h";
                case 0xb92: return "mhpmcounter18h";
                case 0xb93: return "mhpmcounter19h";
                case 0xb94: return "mhpmcounter20h";
                case 0xb95: return "mhpmcounter21h";
                case 0xb96: return "mhpmcounter22h";
                case 0xb97: return "mhpmcounter23h";
                case 0xb98: return "mhpmcounter24h";
                case 0xb99: return "mhpmcounter25h";
                case 0xb9a: return "mhpmcounter26h";
                case 0xb9b: return "mhpmcounter27h";
                case 0xb9c: return "mhpmcounter28h";
                case 0xb9d: return "mhpmcounter29h";
                case 0xb9e: return "mhpmcounter30h";
                case 0xb9f: return "mhpmcounter31h";
                case 0x323: return "mhpmevent3";
                case 0x324: return "mhpmevent4";
                case 0x325: return "mhpmevent5";
                case 0x326: return "mhpmevent6";
                case 0x327: return "mhpmevent7";
                case 0x328: return "mhpmevent8";
                case 0x329: return "mhpmevent9";
                case 0x32a: return "mhpmevent10";
                case 0x32b: return "mhpmevent11";
                case 0x32c: return "mhpmevent12";
                case 0x32d: return "mhpmevent13";
                case 0x32e: return "mhpmevent14";
                case 0x32f: return "mhpmevent15";
                case 0x330: return "mhpmevent16";
                case 0x331: return "mhpmevent17";
                case 0x332: return "mhpmevent18";
                case 0x333: return "mhpmevent19";
                case 0x334: return "mhpmevent20";
                case 0x335: return "mhpmevent21";
                case 0x336: return "mhpmevent22";
                case 0x337: return "mhpmevent23";
                case 0x338: return "mhpmevent24";
                case 0x339: return "mhpmevent25";
                case 0x33a: return "mhpmevent26";
                case 0x33b: return "mhpmevent27";
                case 0x33c: return "mhpmevent28";
                case 0x33d: return "mhpmevent29";
                case 0x33e: return "mhpmevent30";
                case 0x33f: return "mhpmevent31";
                case 0x7a0: return "tselect";
                case 0x7a1: return "tdata1";
                case 0x7a2: return "tdata2";
                case 0x7a3: return "tdata3";
                case 0x7b0: return "dcsr";
                case 0x7b1: return "dpc";
                case 0x7b2: return "dscratch";
                default: return string.Format("0x{0:x3}", csr_addr);
            }
        }
        private string fence_flag(Int16 arg)
        {
            string str;
            str = "";
            if ((arg & (1 << 3)) != 0)
                str = str + "i";
            if ((arg & (1 << 2)) != 0)
                str = str + "o";
            if ((arg & (1 << 1)) != 0)
                str = str + "r";
            if ((arg & (1 << 0)) != 0)
                str = str + "w";
            if (str == "")
                str = str + "-";
            return str;
        }

        private string insn_decode(Int64 pc, Int32 insn, bool rv64, ref bool rd_we, ref Int16 rd, ref bool csr_we, ref Int16 csr, ref bool mem_rd, ref bool mem_wr)
        {
            const Int16 OP_LOAD = 0x0;
            const Int16 OP_LOAD_FP = 0x1;
            const Int16 OP_CUST_0 = 0x2;
            const Int16 OP_MISC_MEM = 0x3;
            const Int16 OP_OP_IMM = 0x4;
            const Int16 OP_AUIPC = 0x5;
            const Int16 OP_OP_IMM_32 = 0x6;
            const Int16 OP_STORE = 0x8;
            const Int16 OP_STORE_FP = 0x9;
            const Int16 OP_CUST_1 = 0xa;
            const Int16 OP_AMO = 0xb;
            const Int16 OP_OP = 0xc;
            const Int16 OP_LUI = 0xd;
            const Int16 OP_OP_32 = 0xe;
            const Int16 OP_MADD = 0x10;
            const Int16 OP_MSUB = 0x11;
            const Int16 OP_NMSUB = 0x12;
            const Int16 OP_NMADD = 0x13;
            const Int16 OP_OP_FP = 0x14;
            const Int16 OP_RSV_0 = 0x15;
            const Int16 OP_CUST_2 = 0x16;
            const Int16 OP_BRANCH = 0x18;
            const Int16 OP_JALR = 0x19;
            const Int16 OP_RSV_1 = 0x1a;
            const Int16 OP_JAL = 0x1b;
            const Int16 OP_SYSTEM = 0x1c;
            const Int16 OP_RSV_2 = 0x1d;
            const Int16 OP_CUST_3 = 0x1e;

            const Int16 OP16_C0 = 0,
                        OP16_C1 = 1,
                        OP16_C2 = 2;

            const Int16 FUNCT3_JALR = 0x0,
                        FUNCT3_BEQ = 0x0,
                        FUNCT3_BNE = 0x1,
                        FUNCT3_BLT = 0x4,
                        FUNCT3_BGE = 0x5,
                        FUNCT3_BLTU = 0x6,
                        FUNCT3_BGEU = 0x7,
                        FUNCT3_LB = 0x0,
                        FUNCT3_LH = 0x1,
                        FUNCT3_LW = 0x2,
                        FUNCT3_LD = 0x3,
                        FUNCT3_LBU = 0x4,
                        FUNCT3_LHU = 0x5,
                        FUNCT3_LWU = 0x6,
                        FUNCT3_SB = 0x0,
                        FUNCT3_SH = 0x1,
                        FUNCT3_SW = 0x2,
                        FUNCT3_SD = 0x3,
                        FUNCT3_ADDI = 0x0,
                        FUNCT3_SLTI = 0x2,
                        FUNCT3_SLTIU = 0x3,
                        FUNCT3_XORI = 0x4,
                        FUNCT3_ORI = 0x6,
                        FUNCT3_ANDI = 0x7,
                        FUNCT3_SLLI = 0x1,
                        FUNCT3_SRLI = 0x5,
                        FUNCT3_SRAI = 0x5,
                        FUNCT3_ADD = 0x0,
                        FUNCT3_SUB = 0x0,
                        FUNCT3_SLL = 0x1,
                        FUNCT3_SLT = 0x2,
                        FUNCT3_SLTU = 0x3,
                        FUNCT3_XOR = 0x4,
                        FUNCT3_SRL = 0x5,
                        FUNCT3_SRA = 0x5,
                        FUNCT3_OR = 0x6,
                        FUNCT3_AND = 0x7,
                        FUNCT3_FENCE = 0x0,
                        FUNCT3_FENCE_I = 0x1,
                        FUNCT3_PRIV = 0x0,
                        FUNCT3_CSRRW = 0x1,
                        FUNCT3_CSRRS = 0x2,
                        FUNCT3_CSRRC = 0x3,
                        FUNCT3_CSRRWI = 0x5,
                        FUNCT3_CSRRSI = 0x6,
                        FUNCT3_CSRRCI = 0x7,
                        FUNCT3_MUL = 0x0,
                        FUNCT3_MULH = 0x1,
                        FUNCT3_MULHSU = 0x2,
                        FUNCT3_MULHU = 0x3,
                        FUNCT3_DIV = 0x4,
                        FUNCT3_DIVU = 0x5,
                        FUNCT3_REM = 0x6,
                        FUNCT3_REMU = 0x7;

            const Int16 FUNCT5_LR = 0x02,
                        FUNCT5_SC = 0x03,
                        FUNCT5_AMOSWAP = 0x01,
                        FUNCT5_AMOADD = 0x00,
                        FUNCT5_AMOXOR = 0x04,
                        FUNCT5_AMOAND = 0x0c,
                        FUNCT5_AMOOR = 0x08,
                        FUNCT5_AMOMIN = 0x10,
                        FUNCT5_AMOMAX = 0x14,
                        FUNCT5_AMOMINU = 0x18,
                        FUNCT5_AMOMAXU = 0x1c;

            const Int16 FUNCT7_SLLI = 0x00,
                        FUNCT7_SRLI = 0x00,
                        FUNCT7_SRAI = 0x20,
                        FUNCT7_OP0 = 0x00,
                        FUNCT7_OP1 = 0x20,
                        FUNCT7_SFENCE_VMA = 0x09,
                        FUNCT7_MULDIV = 0x01;

            const Int16 FUNCT12_ECALL = 0x000,
                        FUNCT12_EBREAK = 0x001,
                        FUNCT12_WFI = 0x105,
                        FUNCT12_SRET = 0x102,
                        FUNCT12_MRET = 0x302;


            const Int16 FUNCT3_C0_ADDI4SPN = 0x0,
                        FUNCT3_C0_FLD = 0x1,
                        FUNCT3_C0_LW = 0x2,
                        FUNCT3_C0_FLW = 0x3,
                        FUNCT3_C0_FSD = 0x5,
                        FUNCT3_C0_SW = 0x6,
                        FUNCT3_C0_FSW = 0x7;

            const Int16 FUNCT3_C1_ADDI = 0x0,
                        FUNCT3_C1_JAL = 0x1,
                        FUNCT3_C1_LI = 0x2,
                        FUNCT3_C1_LUI = 0x3,
                        FUNCT3_C1_OP = 0x4,
                        FUNCT3_C1_J = 0x5,
                        FUNCT3_C1_BEQZ = 0x6,
                        FUNCT3_C1_BNEZ = 0x7;

            const Int16 FUNCT3_C2_SLLI = 0x0,
                        FUNCT3_C2_FLDSP = 0x1,
                        FUNCT3_C2_LWSP = 0x2,
                        FUNCT3_C2_FLWSP = 0x3,
                        FUNCT3_C2_OP = 0x4,
                        FUNCT3_C2_FSDSP = 0x5,
                        FUNCT3_C2_SWSP = 0x6,
                        FUNCT3_C2_FSWSP = 0x7;

            const Int16 FUNCT2_OP_IMM_C_SRLI = 0x0,
                        FUNCT2_OP_IMM_C_SRAI = 0x1,
                        FUNCT2_OP_IMM_C_ANDI = 0x2,
                        FUNCT2_OP_IMM_C_OP = 0x3;

            const Int16 FUNCT2_OP_C_SUB = 0x0,
                        FUNCT2_OP_C_XOR = 0x1,
                        FUNCT2_OP_C_OR = 0x2,
                        FUNCT2_OP_C_AND = 0x3,
                        FUNCT2_OP_C_SUBW = 0x0,
                        FUNCT2_OP_C_ADDW = 0x1;


            Int16 REG_ZERO = 0;
            Int16 REG_RA = 1;
            Int16 REG_SP = 2;
            Int16 REG_GP = 3;
            Int16 REG_TP = 4;
            Int16 REG_T0 = 5;
            Int16 REG_T1 = 6;
            Int16 REG_T2 = 7;
            Int16 REG_S0 = 8;
            Int16 REG_FP = 8;
            Int16 REG_S1 = 9;
            Int16 REG_A0 = 10;
            Int16 REG_A1 = 11;
            Int16 REG_A2 = 12;
            Int16 REG_A3 = 13;
            Int16 REG_A4 = 14;
            Int16 REG_A5 = 15;
            Int16 REG_A6 = 16;
            Int16 REG_A7 = 17;
            Int16 REG_S2 = 18;
            Int16 REG_S3 = 19;
            Int16 REG_S4 = 20;
            Int16 REG_S5 = 21;
            Int16 REG_S6 = 22;
            Int16 REG_S7 = 23;
            Int16 REG_S8 = 24;
            Int16 REG_S9 = 25;
            Int16 REG_S10 = 26;
            Int16 REG_S11 = 27;
            Int16 REG_T3 = 28;
            Int16 REG_T4 = 29;
            Int16 REG_T5 = 30;
            Int16 REG_T6 = 31;


            Int16 rs1, rs2, funct3, funct5, funct7, funct3_16, funct2_16_op_imm, funct2_16_op, opcode_32, opcode_16, pred, succ, shamt;
            Int32 imm_i, imm_s, imm_b, imm_u, imm_j;
            Int32 imm_ci_lwsp, imm_ci_ldsp, imm_ci_li, imm_ci_lui, imm_ci_addi16sp, imm_css, imm_css64, imm_ciw, imm_cl, imm_cl64, imm_cs, imm_cb, imm_cj;
            bool aq, rl;

            rs1 = (Int16)((insn >> 15) & 0x1f);
            rs2 = (Int16)((insn >> 20) & 0x1f);
            rd = (Int16)((insn >> 7) & 0x1f);
            csr = (Int16)((insn >> 20) & 0xfff);

            imm_i = insn >> 20;
            imm_s = ((insn >> 20) & ~0x1f) | ((insn >> 7) & 0x1f);
            imm_b = ((insn >> 20) & ~0x81f) | ((insn >> 7) & 0x1e) | ((insn << 4) & 0x800);
            imm_u = (insn >> 12) << 12;
            imm_j = ((insn >> 31) << 20) | (((insn >> 12) & 0xff) << 12) | (((insn >> 20) & 1) << 11) | (((insn >> 21) & 0x3ff) << 1);

            imm_ci_lwsp = (((insn >> 2) & 0x3) << 6) | (((insn >> 12) & 0x1) << 5) | (((insn >> 4) & 0x7) << 2);
            imm_ci_ldsp = (((insn >> 2) & 0x7) << 6) | (((insn >> 12) & 0x1) << 5) | (((insn >> 5) & 0x3) << 3);
            imm_ci_li = (((insn << 19) >> 31) << 5) | ((insn >> 2) & 0x1f);
            imm_ci_lui = (((insn << 19) >> 31) << 17) | (((insn >> 2) & 0x1f) << 12);
            imm_ci_addi16sp = (((insn << 19) >> 31) << 9) | (((insn >> 3) & 0x3) << 7) | (((insn >> 5) & 0x1) << 6) | (((insn >> 2) & 0x1) << 5) | (((insn >> 6) & 0x1) << 4);
            imm_css = (((insn >> 7) & 0x3) << 6) | (((insn >> 9) & 0xf) << 2);
            imm_css64 = (((insn >> 7) & 0x7) << 6) | (((insn >> 10) & 0x7) << 3);
            imm_ciw = (((insn >> 7) & 0xf) << 6) | (((insn >> 11) & 0x3) << 4) | (((insn >> 5) & 0x1) << 3) | (((insn >> 6) & 0x1) << 2);
            imm_cl = (((insn >> 5) & 0x1) << 6) | (((insn >> 10) & 0x7) << 3) | (((insn >> 6) & 0x1) << 2);
            imm_cl64 = (((insn >> 6) & 0x1) << 7) | (((insn >> 5) & 0x1) << 6) | (((insn >> 10) & 0x7) << 3);
            imm_cs = (((insn >> 5) & 0x1) << 6) | (((insn >> 10) & 0x7) << 3) | (((insn >> 6) & 0x1) << 2);
            imm_cb = (((insn << 19) >> 31) << 8) | (((insn >> 5) & 0x3) << 6) | (((insn >> 2) & 0x1) << 5) | (((insn >> 10) & 0x3) << 3) | (((insn >> 3) & 0x3) << 1);
            imm_cj = (((insn << 19) >> 31) << 11) | (((insn >> 8) & 0x1) << 10) | (((insn >> 9) & 0x3) << 8) |
                (((insn >> 6) & 0x1) << 7) | (((insn >> 7) & 0x1) << 6) | (((insn >> 2) & 0x1) << 5) |
                (((insn >> 11) & 0x1) << 4) | (((insn >> 3) & 0x7) << 1);

            funct3 = (Int16)((insn >> 12) & 0x7);
            funct5 = (Int16)((insn >> 27) & 0x1f);
            funct7 = (Int16)((insn >> 25) & 0x7f);

            aq = (insn & (1 << 26)) != 0;
            rl = (insn & (1 << 25)) != 0;
            funct3_16 = (Int16)((insn >> 13) & 0x7);
            funct2_16_op_imm = (Int16)((insn >> 10) & 0x3);
            funct2_16_op = (Int16)((insn >> 5) & 0x3);

            opcode_16 = (Int16)((insn) & 0x3);
            opcode_32 = (Int16)((insn >> 2) & 0x1f);

            pred = (Int16)((insn >> 24) & 0xf);
            succ = (Int16)((insn >> 20) & 0xf);

            shamt = (Int16)((insn >> 20) & 0x3f);

            csr_we = false;
            rd_we = false;
            mem_rd = false;
            mem_wr = false;


            string str = "Unknown instruction";
            switch (opcode_16)
            {
                case OP16_C0:
                    rs1 = (Int16)((1 << 3) | ((insn >> 7) & 0x7));
                    rs2 = (Int16)((1 << 3) | ((insn >> 2) & 0x7));
                    rd = (Int16)((1 << 3) | ((insn >> 2) & 0x7));
                    rd_we = rd != 0;
                    switch (funct3_16)
                    {
                        case FUNCT3_C0_ADDI4SPN: return string.Format("c.addi4spn {0:},{1:},{2:}", regs_name(rd), regs_name(REG_SP), imm_ciw);
                        case FUNCT3_C0_FLD: return "illegal inst";
                        case FUNCT3_C0_LW: mem_rd = true; return string.Format("c.lw {0:},{1:}({2:})", regs_name(rd), imm_cl, regs_name(rs1));
                        case FUNCT3_C0_FLW: mem_rd = true; return string.Format("c.ld {0:},{1:}({2:})", regs_name(rd), imm_cl64, regs_name(rs1));
                        case FUNCT3_C0_FSD: return "illegal inst";
                        case FUNCT3_C0_SW: mem_wr = true; return string.Format("c.sw {0:},{1:}({2:})", regs_name(rs2), imm_cs, regs_name(rs1));
                        case FUNCT3_C0_FSW: mem_wr = true; return string.Format("c.sd {0:},{1:}({2:})", regs_name(rs2), imm_cl64, regs_name(rs1));
                        default: return "illegal inst";
                    }
                case OP16_C1:
                    rs1 = (Int16)((1 << 3) | ((insn >> 7) & 0x7));
                    rs2 = (Int16)((1 << 3) | ((insn >> 2) & 0x7));
                    rd = (Int16)((1 << 3) | ((insn >> 7) & 0x7));
                    switch (funct3_16)
                    {
                        case FUNCT3_C1_ADDI:
                            rs1 = (Int16)(((insn >> 7) & 0x1f));
                            rd = (Int16)(((insn >> 7) & 0x1f));
                            if (rd == 0 && imm_ci_li == 0) return "c.nop";
                            else rd_we = rd != 0; return string.Format("c.addi {0:},{1:}", regs_name(rd), imm_ci_li);
                        case FUNCT3_C1_JAL:
                            rs1 = (Int16)(((insn >> 7) & 0x1f));
                            rd = (Int16)(((insn >> 7) & 0x1f));
                            rd_we = rd != 0;
                            if (rv64) return string.Format("c.addiw {0:},{1:}", regs_name(rd), imm_ci_li);
                            else return string.Format("c.jal {:x8}", (UInt64)(pc + imm_cj));
                        case FUNCT3_C1_LI:
                            rd = (Int16)(((insn >> 7) & 0x1f));
                            rd_we = rd != 0;
                            return string.Format("c.li {0:},{1:}", regs_name(rd), imm_ci_li);
                        case FUNCT3_C1_LUI:
                            rs1 = (Int16)(((insn >> 7) & 0x1f));
                            rd = (Int16)(((insn >> 7) & 0x1f));
                            rd_we = rd != 0;
                            if (rd == REG_SP) return string.Format("c.addi16sp {0:},{1:}", regs_name(REG_SP), imm_ci_addi16sp);
                            else return string.Format("c.lui {0:},0x{1:x}", regs_name(rd), imm_ci_lui >> 12);
                        case FUNCT3_C1_OP:
                            rd_we = rd != 0;
                            switch (funct2_16_op_imm)
                            {
                                case FUNCT2_OP_IMM_C_SRLI: return string.Format("c.srli {0:},0x{1:x}", regs_name(rd), imm_ci_li & 0x1f);
                                case FUNCT2_OP_IMM_C_SRAI: return string.Format("c.srai {0:},0x{1:x}", regs_name(rd), imm_ci_li & 0x1f);
                                case FUNCT2_OP_IMM_C_ANDI: return string.Format("c.andi {0:},0x{1:x}", regs_name(rd), (UInt32)imm_ci_li);
                                case FUNCT2_OP_IMM_C_OP:
                                    if ((insn & (1 << 12)) == 0)
                                    {
                                        switch (funct2_16_op)
                                        {
                                            case FUNCT2_OP_C_SUB: return string.Format("c.sub {0:},{1:}", regs_name(rd), regs_name(rs2));
                                            case FUNCT2_OP_C_XOR: return string.Format("c.xor {0:},{1:}", regs_name(rd), regs_name(rs2));
                                            case FUNCT2_OP_C_OR: return string.Format("c.or {0:},{1:}", regs_name(rd), regs_name(rs2));
                                            case FUNCT2_OP_C_AND: return string.Format("c.and {0:},{1:}", regs_name(rd), regs_name(rs2));
                                            default: return "illegal inst";
                                        }
                                    }
                                    else
                                    {
                                        switch (funct2_16_op)
                                        {
                                            case FUNCT2_OP_C_SUBW: return string.Format("c.subw {0:},{1:}", regs_name(rd), regs_name(rs2));
                                            case FUNCT2_OP_C_ADDW: return string.Format("c.addw {0:},{1:}", regs_name(rd), regs_name(rs2));
                                            default: return "illegal inst";
                                        }
                                    }

                                default: return "illegal inst";
                            }
                        case FUNCT3_C1_J: return string.Format("c.j {0:x8}", (UInt64)(pc + imm_cj));
                        case FUNCT3_C1_BEQZ: return string.Format("c.beqz {0:},{1:x8}", regs_name(rs1), (UInt64)(pc + imm_cb));
                        case FUNCT3_C1_BNEZ: return string.Format("c.bnez {0:},{1:x8}", regs_name(rs1), (UInt64)(pc + imm_cb));
                        default: return "illegal inst";
                    }
                case OP16_C2:
                    rs1 = (Int16)(((insn >> 7) & 0x1f));
                    rs2 = (Int16)(((insn >> 2) & 0x1f));
                    rd = (Int16)(((insn >> 7) & 0x1f));
                    rd_we = rd != 0;
                    switch (funct3_16)
                    {
                        case FUNCT3_C2_SLLI: return string.Format("c.slli {0:},0x{1:x}", regs_name(rd), imm_ci_li & 0x1f);
                        case FUNCT3_C2_FLDSP: return "illegal inst";
                        case FUNCT3_C2_LWSP: mem_rd = true; return string.Format("c.lwsp {0:},{1:}({2:})", regs_name(rd), imm_ci_lwsp, regs_name(REG_SP));
                        case FUNCT3_C2_FLWSP:
                            mem_rd = true;
                            if (rv64) return string.Format("c.ldsp {0:},{1:}({2:})", regs_name(rd), imm_ci_ldsp, regs_name(REG_SP));
                            return "illegal inst";
                        case FUNCT3_C2_OP:
                            if ((insn & (1 << 12)) == 0)
                            {
                                if (rs2 == REG_ZERO)
                                {
                                    rd_we = false; return string.Format("c.jr {0:}", regs_name(rs1));
                                }
                                else return string.Format("c.mv {0:},{1:}", regs_name(rd), regs_name(rs2));
                            }
                            else
                            {
                                if (rs2 == REG_ZERO)
                                {
                                    if (rs1 == REG_ZERO) return string.Format("c.ebreak");
                                    else return string.Format("c.jalr {0:}", regs_name(rs1));
                                }
                                else return string.Format("c.add {0:},{1:}", regs_name(rd), regs_name(rs2));
                            }
                        case FUNCT3_C2_FSDSP: return "illegal inst";
                        case FUNCT3_C2_SWSP: mem_wr = true; rd_we = false; return string.Format("c.swsp {0:},{1:}({2:})", regs_name(rs2), imm_css, regs_name(REG_SP));
                        case FUNCT3_C2_FSWSP:
                            rd_we = false;
                            mem_wr = true;
                            if (rv64) return string.Format("c.sdsp {0:},{1:}({2:})", regs_name(rs2), imm_css64, regs_name(REG_SP));
                            else return "illegal inst";
                        default: return "illegal inst";
                    }
                default:
                    switch (opcode_32)
                    {
                        case OP_LOAD:
                            rd_we = rd != 0;
                            mem_rd = true;
                            switch (funct3)
                            {
                                case FUNCT3_LB: return string.Format("lb {0:},{1:}({2:})", regs_name(rd), imm_i, regs_name(rs1));
                                case FUNCT3_LH: return string.Format("lh {0:},{1:}({2:})", regs_name(rd), imm_i, regs_name(rs1));
                                case FUNCT3_LW: return string.Format("lw {0:},{1:}({2:})", regs_name(rd), imm_i, regs_name(rs1));
                                case FUNCT3_LBU: return string.Format("lbu {0:},{1:}({2:})", regs_name(rd), imm_i, regs_name(rs1));
                                case FUNCT3_LHU: return string.Format("lhu {0:},{1:}({2:})", regs_name(rd), imm_i, regs_name(rs1));
                                case FUNCT3_LWU: return string.Format("lwu {0:},{1:}({2:})", regs_name(rd), imm_i, regs_name(rs1));
                                case FUNCT3_LD: return string.Format("ld {0:},{1:}({2:})", regs_name(rd), imm_i, regs_name(rs1));
                                default: return "illegal inst";
                            }
                        case OP_LOAD_FP: return "illegal inst";
                        case OP_CUST_0: return "illegal inst";
                        case OP_MISC_MEM:
                            if (((insn >> 7) & 0x1f) != 0 || ((insn >> 15) & 0x1f) != 0)
                                return "illegal inst";
                            switch (funct3)
                            {
                                case FUNCT3_FENCE:
                                    if ((insn >> 28) == 0)
                                    {
                                        if (pred != 0xf || succ != 0xf) return string.Format("fence {0:},{1:}", fence_flag(pred), fence_flag(succ));
                                        else return "fence";
                                    }
                                    else if (((insn >> 28) & 0xf) == 0x8)
                                    {
                                        if (pred != 0xf || succ != 0xf) return string.Format("fence.tso {0:},{1:}", fence_flag(pred), fence_flag(succ));
                                        else return "fence.tso";
                                    }
                                    else return "illegal inst";
                                case FUNCT3_FENCE_I:
                                    if ((insn >> 20) == 0) return "fence.i";
                                    else return "illegal inst";
                                default: return "illegal inst";
                            }
                        case OP_OP_IMM:
                            rd_we = rd != 0;
                            switch (funct3)
                            {
                                case FUNCT3_ADDI:
                                    if (rd == 0 && rs1 == 0 && imm_i == 0) return "nop";
                                    else if (rs1 == 0) return string.Format("li {0:},{1:}", regs_name(rd), imm_i);
                                    else if (imm_i == 0) return string.Format("mv {0:},{1:}", regs_name(rd), regs_name(rs1));
                                    else return string.Format("addi {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), imm_i);
                                case FUNCT3_SLTI: return string.Format("slti {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), imm_i);
                                case FUNCT3_SLTIU: return string.Format("sltiu {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), imm_i);
                                case FUNCT3_XORI:
                                    if (imm_i == -1) return string.Format("not {0:},{1:}", regs_name(rd), regs_name(rs1));
                                    else return string.Format("xori {0:},{1:},0x{2:x}", regs_name(rd), regs_name(rs1), imm_i);
                                case FUNCT3_ORI: return string.Format("ori {0:},{1:},0x{2:x}", regs_name(rd), regs_name(rs1), imm_i);
                                case FUNCT3_ANDI:
                                    return string.Format("andi {0:},{1:},0x{2:x}", regs_name(rd), regs_name(rs1), imm_i);
                                case FUNCT3_SLLI:
                                    if ((funct7 & 0x7e) == (FUNCT7_SLLI & 0x7e))
                                        return string.Format("slli {0:},{1:},0x{2:x}", regs_name(rd), regs_name(rs1), shamt);
                                    else return "illegal inst";
                                case FUNCT3_SRLI:
                                    if ((funct7 & 0x7e) == (FUNCT7_SRLI & 0x7e))
                                        return string.Format("srli {0:},{1:},0x{2:x}", regs_name(rd), regs_name(rs1), shamt);
                                    else if ((funct7 & 0x7e) == (FUNCT7_SRAI & 0x7e))
                                        return string.Format("srai {0:},{1:},0x{2:x}", regs_name(rd), regs_name(rs1), shamt);
                                    else return "illegal inst";
                                default: return "illegal inst";
                            }
                        case OP_AUIPC:
                            rd_we = rd != 0;
                            return string.Format("auipc {0:},0x{1:x}", regs_name(rd), (imm_u >> 12) & 0xfffff);
                        case OP_OP_IMM_32:
                            rd_we = rd != 0;
                            switch (funct3)
                            {
                                case FUNCT3_ADDI:
                                    if (imm_i == 0) return string.Format("sext.w {0:},{1:}", regs_name(rd), regs_name(rs1));
                                    else return string.Format("addiw {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), imm_i);
                                case FUNCT3_SLLI:
                                    if ((funct7 & 0x3e) == (FUNCT7_SLLI & 0x3e)) return string.Format("slliw {0:},{1:},0x{2:x}", regs_name(rd), regs_name(rs1), shamt);
                                    else return "illegal inst";
                                case FUNCT3_SRLI:
                                    if ((funct7 & 0x3e) == (FUNCT7_SRLI & 0x3e)) return string.Format("srliw {0:},{1:},0x{2:x}", regs_name(rd), regs_name(rs1), shamt);
                                    else if ((funct7 & 0x3e) == (FUNCT7_SRAI & 0x3e)) return string.Format("sraiw {0:},{1:},0x{2:x}", regs_name(rd), regs_name(rs1), shamt);
                                    else return "illegal inst";
                                default: return "illegal inst";
                            }
                        case OP_STORE:
                            mem_wr = true;
                            switch (funct3)
                            {
                                case FUNCT3_SB: return string.Format("sb {0:},{1:}({2:})", regs_name(rs2), imm_s, regs_name(rs1));
                                case FUNCT3_SH: return string.Format("sh {0:},{1:}({2:})", regs_name(rs2), imm_s, regs_name(rs1));
                                case FUNCT3_SW: return string.Format("sw {0:},{1:}({2:})", regs_name(rs2), imm_s, regs_name(rs1));
                                case FUNCT3_SD: return string.Format("sd {0:},{1:}({2:})", regs_name(rs2), imm_s, regs_name(rs1));
                                default: return "illegal inst";
                            }
                        case OP_STORE_FP: return "illegal inst";
                        case OP_CUST_1: return "illegal inst";
                        case OP_AMO:
                            rd_we = rd != 0;
                            mem_rd = true;
                            switch (funct5)
                            {
                                case FUNCT5_LR: return string.Format("lr{0:}{1:}{2:} {3:},({4:})", (funct3 == 2) ? ".w" : ".d", aq ? ".aq" : "", rl ? ".rl" : "", regs_name(rd), regs_name(rs1));
                                case FUNCT5_SC: return string.Format("sc{0:}{1:}{2:} {3:},{4:},({5:})", (funct3 == 2) ? ".w" : ".d", aq ? ".aq" : "", rl ? ".rl" : "", regs_name(rd), regs_name(rs2), regs_name(rs1));
                                case FUNCT5_AMOSWAP: return string.Format("amoswap{0:}{1:}{2:} {3:},{4:},({5:})", (funct3 == 2) ? ".w" : ".d", aq ? ".aq" : "", rl ? ".rl" : "", regs_name(rd), regs_name(rs2), regs_name(rs1));
                                case FUNCT5_AMOADD: return string.Format("amoadd{0:}{1:}{2:} {3:},{4:},({5:})", (funct3 == 2) ? ".w" : ".d", aq ? ".aq" : "", rl ? ".rl" : "", regs_name(rd), regs_name(rs2), regs_name(rs1));
                                case FUNCT5_AMOXOR: return string.Format("amoxor{0:}{1:}{2:} {3:},{4:},({5:})", (funct3 == 2) ? ".w" : ".d", aq ? ".aq" : "", rl ? ".rl" : "", regs_name(rd), regs_name(rs2), regs_name(rs1));
                                case FUNCT5_AMOAND: return string.Format("amoand{0:}{1:}{2:} {3:},{4:},({5:})", (funct3 == 2) ? ".w" : ".d", aq ? ".aq" : "", rl ? ".rl" : "", regs_name(rd), regs_name(rs2), regs_name(rs1));
                                case FUNCT5_AMOOR: return string.Format("amoor{0:}{1:}{2:} {3:},{4:},({5:})", (funct3 == 2) ? ".w" : ".d", aq ? ".aq" : "", rl ? ".rl" : "", regs_name(rd), regs_name(rs2), regs_name(rs1));
                                case FUNCT5_AMOMIN: return string.Format("amomin{0:}{1:}{2:} {3:},{4:},({5:})", (funct3 == 2) ? ".w" : ".d", aq ? ".aq" : "", rl ? ".rl" : "", regs_name(rd), regs_name(rs2), regs_name(rs1));
                                case FUNCT5_AMOMAX: return string.Format("amomax{0:}{1:}{2:} {3:},{4:},({5:})", (funct3 == 2) ? ".w" : ".d", aq ? ".aq" : "", rl ? ".rl" : "", regs_name(rd), regs_name(rs2), regs_name(rs1));
                                case FUNCT5_AMOMINU: return string.Format("amominu{0:}{1:}{2:} {3:},{4:},({5:})", (funct3 == 2) ? ".w" : ".d", aq ? ".aq" : "", rl ? ".rl" : "", regs_name(rd), regs_name(rs2), regs_name(rs1));
                                case FUNCT5_AMOMAXU: return string.Format("amomaxu{0:}{1:}{2:} {3:},{4:},({5:})", (funct3 == 2) ? ".w" : ".d", aq ? ".aq" : "", rl ? ".rl" : "", regs_name(rd), regs_name(rs2), regs_name(rs1));
                                default: return "illegal inst";
                            }
                        case OP_OP:
                            rd_we = rd != 0;
                            switch (funct7)
                            {
                                case FUNCT7_OP0:
                                    switch (funct3)
                                    {
                                        case FUNCT3_ADD: return string.Format("add {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_SLL: return string.Format("sll {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_SLT: return string.Format("slt {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_SLTU: return string.Format("sltu {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_XOR: return string.Format("xor {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_SRL: return string.Format("srl {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_OR: return string.Format("or {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_AND: return string.Format("and {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        default: return "illegal inst";
                                    }

                                case FUNCT7_OP1:
                                    switch (funct3)
                                    {
                                        case FUNCT3_ADD: return string.Format("sub {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_SRL: return string.Format("sra {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        default: return "illegal inst";
                                    }

                                case FUNCT7_MULDIV:
                                    switch (funct3)
                                    {
                                        case FUNCT3_MUL: return string.Format("mul {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_MULH: return string.Format("mulh {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_MULHSU: return string.Format("mulhsu {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_MULHU: return string.Format("mulhu {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_DIV: return string.Format("div {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_DIVU: return string.Format("divu {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_REM: return string.Format("rem {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_REMU: return string.Format("remu {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        default: return "illegal inst";
                                    }

                                default: return "illegal inst";
                            }
                        case OP_LUI:
                            rd_we = rd != 0;
                            return string.Format("lui {0:},0x{1:x}", regs_name(rd), imm_u >> 12 & 0xfffff);
                        case OP_OP_32:
                            rd_we = rd != 0;
                            switch (funct7)
                            {
                                case FUNCT7_OP0:
                                    switch (funct3)
                                    {
                                        case FUNCT3_ADD: return string.Format("addw {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_SLL: return string.Format("sllw {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_SRL: return string.Format("srlw {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        default: return "illegal inst";
                                    }

                                case FUNCT7_OP1:
                                    switch (funct3)
                                    {
                                        case FUNCT3_ADD: return string.Format("subw {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_SRL: return string.Format("sraw {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        default: return "illegal inst";
                                    }
                                case FUNCT7_MULDIV:
                                    switch (funct3)
                                    {
                                        case FUNCT3_MUL: return string.Format("mulw {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_DIV: return string.Format("divw {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_DIVU: return string.Format("divuw {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_REM: return string.Format("remw {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        case FUNCT3_REMU: return string.Format("remuw {0:},{1:},{2:}", regs_name(rd), regs_name(rs1), regs_name(rs2));
                                        default: return "illegal inst";
                                    }
                                default: return "illegal inst";
                            }
                        case OP_MADD: return "illegal inst";
                        case OP_MSUB: return "illegal inst";
                        case OP_NMSUB: return "illegal inst";
                        case OP_NMADD: return "illegal inst";
                        case OP_OP_FP: return "illegal inst";
                        case OP_CUST_2: return "illegal inst";
                        case OP_BRANCH:
                            switch (funct3)
                            {
                                case FUNCT3_BEQ:
                                    if (rs2 != 0) return string.Format("beq {0:},{1:},{2:x8}", regs_name(rs1), regs_name(rs2), (UInt64)(pc + imm_b));
                                    else return string.Format("beqz {0:},{1:x8}", regs_name(rs1), (UInt64)(pc + imm_b));
                                case FUNCT3_BNE:
                                    if (rs2 != 0) return string.Format("bne {0:},{1:},{2:x8}", regs_name(rs1), regs_name(rs2), (UInt64)(pc + imm_b));
                                    else return string.Format("bnez {0:},{1:x8}", regs_name(rs1), (UInt64)(pc + imm_b));
                                case FUNCT3_BLT:
                                    if (rs1 == 0) return string.Format("bgtz {0:},{1:x8}", regs_name(rs2), (UInt64)(pc + imm_b));
                                    else if (rs2 == 0) return string.Format("bltz {0:},{1:x8}", regs_name(rs1), (UInt64)(pc + imm_b));
                                    else return string.Format("blt {0:},{1:},{2:x8}", regs_name(rs1), regs_name(rs2), (UInt64)(pc + imm_b));
                                case FUNCT3_BGE:
                                    if (rs1 == 0) return string.Format("blez {0:},{1:x8}", regs_name(rs2), (UInt64)(pc + imm_b));
                                    else if (rs2 == 0) return string.Format("bgez {0:},{1:x8}", regs_name(rs1), (UInt64)(pc + imm_b));
                                    else return string.Format("bge {0:},{1:},{2:x8}", regs_name(rs1), regs_name(rs2), (UInt64)(pc + imm_b));
                                case FUNCT3_BLTU: return string.Format("bltu {0:},{1:},{2:x8}", regs_name(rs1), regs_name(rs2), (UInt64)(pc + imm_b));
                                case FUNCT3_BGEU: return string.Format("bgeu {0:},{1:},{2:x8}", regs_name(rs1), regs_name(rs2), (UInt64)(pc + imm_b));
                                default: return "illegal inst";
                            }
                        case OP_JALR:
                            rd_we = rd != 0;
                            if (imm_i != 0 || !(rd == REG_ZERO || rd == REG_RA)) return string.Format("jalr {0:},{1:}({2:})", regs_name(rd), imm_i, regs_name(rs1));
                            else if (rd == REG_RA) return string.Format("jalr {0:}", regs_name(rs1));
                            else if (rs1 == REG_RA) return "ret";
                            else return string.Format("jr {0:}", regs_name(rs1));
                        case OP_JAL:
                            rd_we = rd != 0;
                            if (rd == 0) return string.Format("j {0:x8}", (UInt64)(pc + imm_j));
                            else if (rd == REG_RA) return string.Format("jal {0:x8}", (UInt64)(pc + imm_j));
                            else return string.Format("jal {0:},{1:x8}", regs_name(rd), (UInt64)(pc + imm_j));
                        case OP_SYSTEM:
                            switch (funct3) {
                                case FUNCT3_PRIV:
                                    if (funct7 == FUNCT7_SFENCE_VMA)
                                        if (rs1 == 0 && rs2 == 0) return "sfence.vma";
                                        else if (rs2 == 0) return string.Format("sfence.vma {0:}", regs_name(rs1));
                                        else return string.Format("sfence.vma {0:},{1:}", regs_name(rs1), regs_name(rs2));
                                    else if (((insn >> 7) & 0x1f) == 0 && ((insn >> 15) & 0x1f) == 0)
                                        switch ((insn >> 20) & 0xfff) {
                                            case FUNCT12_ECALL: return "ecall";
                                            case FUNCT12_EBREAK: return "ebreak";
                                            case FUNCT12_WFI: return "wfi";
                                            case FUNCT12_SRET: return "sret";
                                            case FUNCT12_MRET: return "mret";
                                            default: return "illegal inst";
                                        }
                                    else return "illegal inst";
                                case FUNCT3_CSRRW:
                                    csr_we = true;
                                    rd_we = rd != 0;
                                    if (rd != 0) return string.Format("csrrw {0:},{1:},{2:}", regs_name(rd), csr_name(csr), regs_name(rs1));
                                    else return string.Format("csrw {0:},{1:}", csr_name(csr), regs_name(rs1));
                                case FUNCT3_CSRRS:
                                    csr_we = rs1 != 0;
                                    rd_we = rd != 0;
                                    if (rs1 == 0) return string.Format("csrr {0:},{1:}", regs_name(rd), csr_name(csr));
                                    else if (rd == 0) return string.Format("csrs {0:},{1:}", csr_name(csr), regs_name(rs1));
                                    else return string.Format("csrrs {0:},{1:},{2:}", regs_name(rd), csr_name(csr), regs_name(rs1));
                                case FUNCT3_CSRRC:
                                    csr_we = rs1 != 0;
                                    rd_we = rd != 0;
                                    if (rs1 == 0) return string.Format("csrr {0:},{1:}", regs_name(rd), csr_name(csr));
                                    else if (rd == 0) return string.Format("csrc {0:},{1:}", csr_name(csr), regs_name(rs1));
                                    else return string.Format("csrrc {0:},{1:},{2:}", regs_name(rd), csr_name(csr), regs_name(rs1));
                                case FUNCT3_CSRRWI:
                                    csr_we = true;
                                    rd_we = rd != 0;
                                    if (rd != 0) return string.Format("csrrwi {0:},{1:},0x{2:x}", regs_name(rd), csr_name(csr), rs1);
                                    else return string.Format("csrwi {0:},0x{1:x}", csr_name(csr), rs1);
                                case FUNCT3_CSRRSI:
                                    csr_we = rs1 != 0;
                                    rd_we = rd != 0;
                                    if (rs1 == 0) return string.Format("csrr {0:},{1:}", regs_name(rd), csr_name(csr));
                                    else if (rd == 0) return string.Format("csrsi {0:},0x{1:x}", csr_name(csr), rs1);
                                    else return string.Format("csrrsi {0:},{1:},0x{2:x}", regs_name(rd), csr_name(csr), rs1);
                                case FUNCT3_CSRRCI:
                                    csr_we = rs1 != 0;
                                    rd_we = rd != 0;
                                    if (rs1 == 0) return string.Format("csrr {0:},{1:}", regs_name(rd), csr_name(csr));
                                    else if (rd == 0) return string.Format("csrci {0:},0x{1:x}", csr_name(csr), rs1);
                                    else return string.Format("csrrci {0:},{1:},0x{2:x}", regs_name(rd), csr_name(csr), rs1);
                                default: return "illegal inst";
                            }
                        case OP_CUST_3: return "illegal inst";
                        default: return "illegal inst";
                    }
            }
            return str;
        }
        private string tracer_decode(int[] buff)
        {
            const Int64 CAUSE_MISALIGNED_FETCH = 0x0;
            const Int64 CAUSE_INSTRUCTION_ACCESS = 0x1;
            const Int64 CAUSE_ILLEGAL_INSTRUCTION = 0x2;
            const Int64 CAUSE_BREAKPOINT = 0x3;
            const Int64 CAUSE_MISALIGNED_LOAD = 0x4;
            const Int64 CAUSE_LOAD_ACCESS = 0x5;
            const Int64 CAUSE_MISALIGNED_STORE = 0x6;
            const Int64 CAUSE_STORE_ACCESS = 0x7;
            const Int64 CAUSE_USER_ECALL = 0x8;
            const Int64 CAUSE_SUPERVISOR_ECALL = 0x9;
            const Int64 CAUSE_HYPERVISOR_ECALL = 0xa;
            const Int64 CAUSE_MACHINE_ECALL = 0xb;
            const Int64 CAUSE_INSTRUCTION_PAGE_FAULT = 0xc;
            const Int64 CAUSE_LOAD_PAGE_FAULT = 0xd;
            const Int64 CAUSE_STORE_PAGE_FAULT = 0xf;

            bool trap_en = ((buff[7] >> 30) & 0x3) == 0x3;
            byte prv = (byte)((buff[7] >> 28) & 0x3);
            bool rv64 = ((buff[7] >> 27) & 1) != 0;
            Int64 time = (Int64) (buff[7] & 0x07ffffff);
            Int64 pc = ((Int64)buff[6] << 32) | (Int64)buff[5] & 0xffffffff;
            Int32 insn = buff[4];
            Int64 mcause, csr_wdata, mem_addr;
            Int64 mtval, mem_data, rd_data;
            bool rd_we = false, csr_we = false, mem_rd = false, mem_wr = false;
            Int16 rd = 0, csr = 0;
            string str = "", tmp;

            mcause = csr_wdata = mem_addr = (((Int64)buff[3]) << 32) | (Int64)buff[2] & 0xffffffff;
            mtval = mem_data = rd_data = (((Int64)buff[1]) << 32) | (Int64)buff[0] & 0xffffffff;

            if (trap_en)
            {
                if (mcause >> 31 != 0) str = string.Format("({0:d1} cycles) [{1:}] Interrupt {2:d}, epc = 0x{3:x8}, tval = 0x{4:x8}", time,
                    prv == 0 ? "U" : prv == 1 ? "S" : prv == 3 ? "M" : "X",
                    mcause & 0x7fffffff, pc, mtval);
                else
                {
                    switch (mcause)
                    {
                        case CAUSE_MISALIGNED_FETCH: str = "InstructionAddressMisaligned"; break;
                        case CAUSE_INSTRUCTION_ACCESS: str = "InstructionAccessFault"; break;
                        case CAUSE_ILLEGAL_INSTRUCTION: str = "IllegalInstruction"; break;
                        case CAUSE_BREAKPOINT: str = "Breakpoint"; break;
                        case CAUSE_MISALIGNED_LOAD: str = "LoadAddressMisaligned"; break;
                        case CAUSE_LOAD_ACCESS: str = "LoadAccessFault"; break;
                        case CAUSE_MISALIGNED_STORE: str = "StoreAddressMisaligned"; break;
                        case CAUSE_STORE_ACCESS: str = "StoreAccessFault"; break;
                        case CAUSE_USER_ECALL: str = "UserEcall"; break;
                        case CAUSE_SUPERVISOR_ECALL: str = "SupervisorEcall"; break;
                        case CAUSE_HYPERVISOR_ECALL: str = "HypervisorEcall"; break;
                        case CAUSE_MACHINE_ECALL: str = "MachineEcall"; break;
                        case CAUSE_INSTRUCTION_PAGE_FAULT: str = "InstructionPageFault"; break;
                        case CAUSE_LOAD_PAGE_FAULT: str = "LoadPageFault"; break;
                        case CAUSE_STORE_PAGE_FAULT: str = "StorePageFault"; break;
                        default: str = string.Format("Unknown exception #{0:}", mcause); break;
                    }
                    str = string.Format("({0:d1} cycles) [{1:}] {2:}, epc = 0x{3:x8}, tval = 0x{4:x8}", time, 
                        prv == 0 ? "U" : prv == 1 ? "S" : prv == 3 ? "M" : "X", str, pc, mtval);
                }
            }
            else
            {
                str = insn_decode(pc, insn, rv64, ref rd_we, ref rd, ref csr_we, ref csr, ref mem_rd, ref mem_wr);
                tmp = (insn & 3) == 3 ? string.Format("{0:x8}", insn) : string.Format("----{0:x4}", insn);
                str = string.Format("({0:d1} cycles) [{1:}] {2:x16}:{3:} {4:}", time, prv == 0 ? "U" : prv == 1 ? "S" : prv == 3 ? "M" : "X", (UInt64)pc, tmp, str);
                if (mem_rd) str = str + '\n' + string.Format("  LOAD  MEM[{0:x8}]: {1:x8} {2:x8}", mem_addr, (mem_data >> 32) & 0xffffffff, mem_data & 0xffffffff);
                else if (mem_wr) str = str + '\n' + string.Format("  STORE MEM[{0:x8}]: {1:x8} {2:x8}", mem_addr, (mem_data >> 32) & 0xffffffff, mem_data & 0xffffffff);
                if (csr_we) str = str + '\n' + string.Format("  {0:}{1:x16}", csr_name(csr).PadRight(10, ' '), csr_wdata);
                if (rd_we) str = str + '\n' + string.Format("  {0:}{1:x16}", regs_name(rd).PadRight(10, ' '), rd_data);
            }
            return str;
        }

        private void trace_btn_Click(object sender, EventArgs e)
        {
            string filepath;
            DAP.DAPCMDPacket pkg;
            pkg.done = new DAP.ref_bool();
            Int32 [] data = new Int32[0x40];
            Int32[] _data = new Int32[0x8];
            Int32[] resp = new Int32[0x4] { -1, -1, -1, -1 };

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text|*.txt";
                saveFileDialog.Title = "Save trace file";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filepath = saveFileDialog.FileName;
                    using (StreamWriter writer = new StreamWriter(filepath))
                    {
                        for (int j = 0; j < 0x1000; j += 0x40 * 4)
                        {
                            pkg.cmd = DAP.DAPCMD.AXIReadBatch;
                            pkg.addr = 0x04002000 + j;
                            pkg.size = 2;
                            pkg.len = 0x40;
                            pkg.dbuff = data;
                            pkg.rbuff = resp;
                            pkg.done.val = false;
                            dap.cmd_enqueue(pkg);
                            while (!pkg.done.val) Thread.Sleep(100);
                            for (int i = 0; i < 0x40; i += 8)
                            {
                                Array.Copy(data, i, _data, 0, 8);
                                writer.WriteLine(tracer_decode(_data));
                            }
                        }
                    }
                }
            }
        }
    }
}
