using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Oodles
{
    public class DisplayForm : Form
    {
        private Device device;
        private PresentParameters presentParams;

        private bool fullscreen = false;

        public DisplayForm(string title, Size resolution, bool fullscreen)
        {
            this.Text = title;
            this.ClientSize = resolution;

            this.fullscreen = fullscreen;

            if (fullscreen)
            {
                this.FormBorderStyle = FormBorderStyle.None;
            }
        }

        public void InitializeGraphics()
        {
            try
            {
                // Setup DirectX device
                presentParams = new PresentParameters();

                // Always use windowed mode for debugging and fullscreen for.
                presentParams.Windowed = !fullscreen;

                presentParams.PresentationInterval = PresentInterval.Default;
                presentParams.BackBufferFormat = Format.X8R8G8B8;
                presentParams.BackBufferWidth = this.ClientSize.Width;
                presentParams.BackBufferHeight = this.ClientSize.Height;

                // Default to triple buffering for performance gain, if we are low on video memory and use multisampling, 1 is ok too.
                presentParams.BackBufferCount = 1;

                // Discard back buffer when swapping, its faster
                presentParams.SwapEffect = SwapEffect.Discard;

                // No multisampling yet ...
                presentParams.MultiSample = MultiSampleType.None;
                // Doesn't work perfectly in fullscreen (often out of memory exceptions).
                // If you want to use multi sampling you should enumerate all adapters first!
                // Doesn't makes much of a difference anyway because the post screen blur reduces any aliasing anyway.
                presentParams.MultiSampleQuality = 0;

                // Use a Z-Buffer with 32 bit if possible
                presentParams.EnableAutoDepthStencil = true;
                presentParams.AutoDepthStencilFormat = DepthFormat.D24X8;//D32;

                // Create device and set some render states.
                try
                {
                    device = new Device(
                        0,
                        DeviceType.Hardware,
                        this,
                        CreateFlags.HardwareVertexProcessing,
                        presentParams);
                }
                catch (Exception)
                {

                }
                
                this.device.DeviceReset += new EventHandler(OnDeviceReset);
                this.device.DeviceLost += new EventHandler(OnDeviceLost);
                this.device.DeviceResizing += new CancelEventHandler(OnDeviceResizing);
                this.device.Disposing += new EventHandler(OnDeviceDisposing);
                
                // Initialize device settings for the first time.
                OnDeviceReset(device, null);

                // Get device caps
                Caps caps = Manager.GetDeviceCaps(0, DeviceType.Hardware);

                // We need at least ps1.1, ps2.0 is even better
                if (caps.PixelShaderVersion.Major < 1)
                    throw new DirectXException("Get a better graphicscard (this one doesn't even have PixelShader 1.1)");
            }
            catch (DirectXException ex)
            {
                // Pass error 1 level up to the main program.
                throw new Exception(
                    "Unable to create DirectX device for this game (software " +
                    "rendering is not supported, it is way to slow).\n" +
                    "Your graphic card does most likely do not support DirectX 7, 8 " +
                    "or 9 (the game works with ps2.0, ps1.1 or no shaders at all).\n" +
                    "Further information can only be determinated in the debug " +
                    "mode.\n" + "Error: " + ex.Message, ex);
            }
        }

        private void OnDeviceReset(object sender, EventArgs e)
        {
            if (device == null || device.Disposed)
                return;

            // Create view and projection matrices (default stuff)
            this.Device.Transform.View = Matrix.LookAtLH(
                new Vector3(0.0f, 0.0f, -1.0f), // Position
                new Vector3(0.0f, 0.0f, 0.0f), // LookAt
                new Vector3(0.0f, 1.0f, 0.0f)); // Up

            float aspectRatio = (float)this.ClientSize.Width / (float)this.ClientSize.Height;
            float fieldOfView = (float)Math.PI / 4.0f;
            float nearPlane = 1.0f;
            float farPlane = 1000.0f;

            this.Device.Transform.Projection = Matrix.PerspectiveFovLH(
                fieldOfView, aspectRatio, nearPlane, farPlane);
            /*
            this.Device.Lights[0].Diffuse = System.Drawing.Color.White;
            this.Device.Lights[0].Type = LightType.Directional;
            this.Device.Lights[0].Direction = new Vector3(0, -1, 1);
            this.Device.Lights[0].Enabled = true;
            */
            this.device.RenderState.Lighting = false;

            this.device.RenderState.CullMode = Cull.None;

            this.device.VertexFormat = VertexFormats.Position | VertexFormats.Diffuse;

            this.device.RenderState.AlphaBlendEnable = true;
            //this.device.RenderState.SourceBlend = Blend.SourceAlpha;
            //this.device.RenderState.DestinationBlend = Blend.DestinationAlpha;

            this.device.RenderState.PointSpriteEnable = true;
            this.device.RenderState.PointScaleEnable = true;
        }

        private void OnDeviceResizing(object sender, EventArgs e)
        {
            if (device == null)
                return;
        }

        private void OnDeviceLost(object sender, EventArgs e)
        {

        }

        private void OnDeviceDisposing(object sender, EventArgs e)
        {
            if (device == null || device.Disposed)
                return;
        }

        /// <summary>
        /// As long as the application is idle update/PreRender/Render
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnApplicationIdle(object sender, EventArgs e)
        {
            while (this.AppStillIdle)
            {
                OnFrame();
                Render();
            }
        }

        protected virtual void Render()
        {
            // override
        }

        protected virtual void OnFrame()
        {
            // override
        }

        /// <summary>
        /// AppStillIdle property that checks using PeekMessage if the application is still idle
        /// </summary>
        public bool AppStillIdle
        {
            get
            {
                NativeMethods.Message msg;
                return !NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
            }
        }

        public Device Device
        {
            get
            {
                return device;
            }
        }
    }
}