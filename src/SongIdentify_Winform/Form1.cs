using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace SongIdentify_Winform
{
    public partial class Form1 : Form
    {
        Thread t_watcher;
        IntPtr hWnd;
        int processID;
        Process currProcess;

        public Form1()
        {
            InitializeComponent();

            t_watcher = new Thread(new ThreadStart(Monitor));
            t_watcher.IsBackground = true; 
            t_watcher.Start();
        }

        /// <summary>
        /// Background thread that monitors running audio session & update labeltext.
        /// </summary>
        void Monitor()
        {
            int sessionCnt = 0;
            while (true)
            {
                Thread.Sleep(1000);
                foreach (AudioSession item in AudioUtilities.GetAllSessions())
                {
                    if (item.Process == null || IntPtr.Equals(item.Process.MainWindowHandle, IntPtr.Zero))
                    {
                        item.Dispose();
                        continue;
                    }
                    
                    item.Process.Refresh();
                    //if audio playback active.
                    if (item.State == AudioSessionState.Active)
                    {
                        sessionCnt += 1;
                        try
                        {                                           
                            if (item.Process.ProcessName.Equals("spotify", StringComparison.InvariantCultureIgnoreCase))
                            {
                                UpdateLabel_T(item.Process.MainWindowTitle);
                            }
                            else if (item.Process.ProcessName.Equals("foobar2000", StringComparison.InvariantCultureIgnoreCase))
                            {
                                //UpdateLabel_T(item.Process.MainWindowTitle.Replace("[foobar2000]", ""));
                                UpdateLabel_T(item.Process.MainWindowTitle.Substring(0, item.Process.MainWindowTitle.Length - 12)); //cheaper.
                            }
                            else if (item.Process.ProcessName.Equals("vlc", StringComparison.InvariantCultureIgnoreCase))
                            {
                                UpdateLabel_T(item.Process.MainWindowTitle.Substring(0, item.Process.MainWindowTitle.Length - 19));
                            }
                            else if (item.Process.ProcessName.Equals("chrome", StringComparison.InvariantCultureIgnoreCase))
                            {
                                // won't update if video changes & different tab open on browser.
                                if (item.Process.MainWindowTitle.Contains(" - YouTube - Google Chrome"))
                                    UpdateLabel_T(item.Process.MainWindowTitle.Substring(0, item.Process.MainWindowTitle.Length - 26));
                            }
                            else if (item.Process.ProcessName.Equals("firefox", StringComparison.InvariantCultureIgnoreCase))
                            {
                                //failed to find correct process for firefox, finding windowhandle instead.
                                hWnd = StaticPinvoke.FindWindow("MozillaWindowClass", null);
                                StaticPinvoke.GetWindowThreadProcessId(hWnd, out processID);
                                currProcess = Process.GetProcessById(processID);

                                // won't update if video changes & different tab open on browser.
                                if (currProcess.MainWindowTitle.Contains(" - YouTube - Mozilla Firefox"))
                                    UpdateLabel_T(currProcess.MainWindowTitle.Substring(0, currProcess.MainWindowTitle.Length - 28));
                            }
                            else if (item.Process.ProcessName.Equals("nvcontainer", StringComparison.InvariantCultureIgnoreCase)) // nvidia gfe screen recording.
                            {
                                sessionCnt -= 1;
                            }
                            else
                            {
                                UpdateLabel_T(item.Process.MainWindowTitle);
                            }
                        }
                        catch(ArgumentOutOfRangeException) //when window title < subtracting value.
                        {
                            UpdateLabel_T(item.Process.MainWindowTitle);
                        }
                    }
                    item.Dispose();
                }
                if(sessionCnt <= 0)
                {
                     UpdateLabel_T("No Playback");
                }

                sessionCnt = 0;
            }
        }

        public void UpdateLabel_T(string title)
        {
            try
            {
                if (!InvokeRequired)
                {
                    songTitleLabel.Text = title;
                }
                else
                {
                    Invoke(new Action<string>(UpdateLabel_T), title);
                }
            }
            catch(Exception)
            {
                
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }

}
