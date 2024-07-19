using MetaFrm.Alert;
using Microsoft.AspNetCore.Components;

namespace MetaFrm.Razor.Alert
{
    /// <summary>
    /// Toast
    /// </summary>
    public partial class Toast
    {
        private static bool IsLoadAttribute = false;
        private static string? CssClassAppendStatic;

        private int runCount = 0;

        /// <summary>
        /// CssClassDiv
        /// </summary>
        [Parameter]
        public string? CssClassAppend { get; set; }
        private string? CssClassAppendOrStatic => this.CssClassAppend ?? CssClassAppendStatic;

        /// <summary>
        /// ToastMessage
        /// </summary>
        [Parameter]
        public MetaFrm.Alert.Toast? ToastMessage { get; set; }


        #region Init
        /// <summary>
        /// FieldControl
        /// </summary>
        public Toast()
        {
            if (!IsLoadAttribute)
            {
                CssClassAppendStatic = this.GetAttribute(nameof(this.CssClassAppend));

                IsLoadAttribute = true;
            }
        }
        #endregion

        /// <summary>
        /// OnAfterRender
        /// </summary>
        /// <param name="firstRender"></param>
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (firstRender)
            { }

            if (this.ToastMessage != null && this.ToastMessage.IsVisible)
                RunTimer();
        }

        /// <summary>
        /// RunTimer
        /// </summary>
        public void RunTimer()
        {
            if (this.ToastMessage != null && this.ToastMessage.IsVisible)
            {
                try
                {
                    Timer timer = new(new TimerCallback(TimerProc));

                    if (this.ToastMessage.Duration == ToastDuration.Short)
                        timer.Change(3000, 0);
                    else
                        timer.Change(6000, 0);
                }
                catch (Exception)
                {
                    this.runCount--;
                }
                finally
                {
                    this.runCount++;
                }
            }
        }
        private void TimerProc(object? state)
        {
            try
            {
                if (this.ToastMessage != null && this.ToastMessage.IsVisible && this.runCount <= 1)
                {
                    this.ToastMessage.IsVisible = false;
                    this.ToastMessage.Text = "";
                    this.ToastMessage.Title = "";

                    this.InvokeAsync(this.StateHasChanged);
                }
            }
            finally
            {
                this.runCount--;
            }
        }

        private void Close()
        {
            if (this.ToastMessage != null)
            {
                this.ToastMessage.IsVisible = false;
                this.ToastMessage.Text = "";
                this.ToastMessage.Title = "";
                this.runCount = 0;
            }
        }
    }
}