using System;
using System.Windows;
using System.Windows.Forms;

namespace HideIt
{


    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private HotKey _hot;
        private bool _isHide = false;
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _hot = new HotKey(this, HotKey.KeyFlags.MOD_WIN, Keys.Escape);
            _hot.OnHotKey += Hot_OnHotKey;
        }

        private void BtnHideIt_Click(object sender, RoutedEventArgs e)
        {
            HideAll(TbName.Text);
        }

        public IntPtr FindToolbar32Wnd()
        {
            var hWnd1 = Win32Api.FindWindow("Shell_TrayWnd", null);
            hWnd1 = Win32Api.FindWindowEx(hWnd1, IntPtr.Zero, "TrayNotifyWnd", null);
            hWnd1 = Win32Api.FindWindowEx(hWnd1, IntPtr.Zero, "SysPager", null);
            hWnd1 = Win32Api.FindWindowEx(hWnd1, IntPtr.Zero, "ToolbarWindow32", null);
            return hWnd1;
        }

        public IntPtr FindOverflowToolbarWnd()
        {
            var hWnd2 = Win32Api.FindWindow("NotifyIconOverflowWindow", null);
            hWnd2 = Win32Api.FindWindowEx(hWnd2, IntPtr.Zero, "ToolbarWindow32", null);
            return hWnd2;
        }

        public void ShowTrayIcon(IntPtr hToolbar, string name,bool isShow)
        {
            if (string.IsNullOrEmpty(name))
                return;
            Win32Api.GetWindowThreadProcessId(hToolbar, out int id);
            int nButtonCount = Win32Api.SendMessage(hToolbar, 0x0400 + 24, 0, 0);
            var hProcess = Win32Api.OpenProcess(Win32Api.PROCESS_ALL_ACCESS
                                                | Win32Api.PROCESS_VM_OPERATION
                                                | Win32Api.PROCESS_VM_READ
                                                | Win32Api.PROCESS_VM_WRITE,
                false,
                id);
            var lpTbbution = Win32Api.VirtualAllocEx(hProcess, 0, 32, 0x1000, 0x04);
            for (int i = 0; i < nButtonCount; i++)
            {
                //获取TBBUTTON信息
                Win32Api.SendMessage(hToolbar, 0x0400 + 23, i, lpTbbution);
                Win32Api.ReadProcessMemory(hProcess, lpTbbution, out TBBUTTON pp, 32, out uint iss);
                //获取标题长度
                var length = Win32Api.SendMessage(hToolbar, 0x0400 + 75, pp.idCommand, 0);
                if (length == 0)
                    continue;
                //获取标题
                var lpTextAddress = Win32Api.VirtualAllocEx(hProcess, 0, length, 0x1000, 0x04);
                Win32Api.SendMessage(hToolbar, 0x0400 + 75, pp.idCommand, lpTextAddress);
                byte[] cs = new byte[522];
                Win32Api.ReadProcessMemory(hProcess, lpTextAddress, cs, 522, out uint ssssss);
                string str = System.Text.Encoding.Unicode.GetString(cs);
                Win32Api.VirtualFreeEx(hProcess, lpTextAddress, (uint) length, 0x10000);
                if (str?.Contains(name.Trim()) == true)
                {
                    //int bHide = Win32Api.SendMessage(hToolbar, 0x0400 + 12, pp.idCommand, 0);
                    int bResult = Win32Api.SendMessage(hToolbar, 0x0400 + 4, pp.idCommand, isShow ? 1 : 0);
                }
            }
            Win32Api.VirtualFreeEx(hProcess, lpTbbution, 32, 0x10000);
            Win32Api.CloseHandle(hProcess);
        }

        private void BtnHideMe_Click(object sender, RoutedEventArgs e)
        {
            this.ShowInTaskbar = false;
            this.Hide();
        }

        private void Hot_OnHotKey()
        {
            HideAll(TbName.Text);
        }

        public void HideAll(string name)
        {
            try
            {
                var p = FindToolbar32Wnd();
                ShowTrayIcon(p, name, !_isHide);
                p = FindOverflowToolbarWnd();
                ShowTrayIcon(p, name, !_isHide);
                var weChat = Win32Api.FindWindow("WeChatMainWndForPC", null);
                if (weChat == IntPtr.Zero)
                {
                    weChat = Win32Api.FindWindow("WeChatMainWndForStore", null);
                }
                if (weChat != IntPtr.Zero)
                {
                    Win32Api.ShowWindow(weChat, _isHide ? 9u : 0u);
                }
                if (_isHide == false)
                {
                    var weChatWeb = Win32Api.FindWindow("CefWebViewWnd", null);
                    if (weChatWeb != IntPtr.Zero)
                    {
                        Win32Api.ShowWindow(weChatWeb, _isHide ? 9u : 0u);
                    }
                }

                _isHide = !_isHide;
            }
            catch (Exception exception)
            {
                ;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox) sender;
            if (cb.Checked)
            {
                _hot.OnHotKey -= Hot_OnHotKey;
            }
            else
            {
                _hot.OnHotKey += Hot_OnHotKey; 
            }
        }
    }
}
