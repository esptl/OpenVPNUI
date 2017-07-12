#region

using System;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

#endregion

namespace Esp.Tools.OpenVPN.SharedUI
{
    public class DropShadowWindow : Window
    {
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr pHwnd, int pAttr, ref int pAttrValue, int pAttrSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr pHWnd, ref DwmMargins pMarInset);

        

        public DropShadowWindow()
        {
            SourceInitialized += new EventHandler(DropShadowWindow_SourceInitialized);
        }

        void DropShadowWindow_SourceInitialized(object sender, EventArgs e)
        {
            if(GlassMargin!=null || AllGlass)
                DropShadow();
        }

      
        [StructLayout(LayoutKind.Sequential)]
        public struct DwmMargins
        {
            public int Left;      // width of left border that retains its size
            public int Right;     // width of right border that retains its size
            public int Top;      // height of top border that retains its size
            public int Bottom;   // height of bottom border that retains its size
        };

        /// <summary>
        ///   The actual method that makes API calls to drop the shadow to the window
        /// </summary>
        /// <param name = "pWindow">Window to which the shadow will be applied</param>
        /// <returns>True if the method succeeded, false if not</returns>
        private bool DropShadow()
        {

            try
            {
                var helper = new WindowInteropHelper(this);
                int val = 2;
                int ret1 = DwmSetWindowAttribute(helper.Handle, 2, ref val, 4);

                if (ret1 == 0)
                {
                    this.Background = Brushes.Transparent;
                    HwndSource.FromHwnd(helper.Handle).CompositionTarget.BackgroundColor = Colors.Transparent;
                    var desktop = System.Drawing.Graphics.FromHwnd(helper.Handle);
                    var m = ConvertMargins(GlassMargin,desktop.DpiX, desktop.DpiY );
                    var ret2 = DwmExtendFrameIntoClientArea(helper.Handle, ref m);
                    return ret2 == 0;
                }
                return false;
            }
            catch (Exception)
            {
                // Probably dwmapi.dll not found (incompatible OS)
                return false;
            }

        }

        private DwmMargins ConvertMargins(Margins pMargins,float DesktopDpiX, float DesktopDpiY)
        {

            if(AllGlass)
                return new DwmMargins()
                           {
                               Left = -1,
                               Right = -1,
                               Bottom = -1,
                               Top = -1
                           };
            return new DwmMargins
                              {
                                  Left = Convert.ToInt32(pMargins.Left*(DesktopDpiX/96)),
                                  Right = Convert.ToInt32(pMargins.Right*(DesktopDpiX/96)),
                                  Top = Convert.ToInt32((pMargins.Top)*(DesktopDpiX/96)),
                                  Bottom = Convert.ToInt32(pMargins.Bottom*(DesktopDpiX/96))
                              };            

        }

        public bool AllGlass { get; set; }

        private Margins _glassMargin;
        public Margins GlassMargin
        {
            get { return _glassMargin; }
            set
            {
                _glassMargin = value;
                
            }
        }
    }
}