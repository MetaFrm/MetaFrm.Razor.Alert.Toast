using MetaFrm.Alert;
using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;

namespace MetaFrm.Razor.Alert
{
    /// <summary>
    /// Toast
    /// </summary>
    public partial class Toast
    {
        private static bool IsLoadAttribute = false;
        private static string? CssClassAppendStatic;

        private readonly ConcurrentQueue<MetaFrm.Alert.Toast> Queue = new();

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

        private MetaFrm.Alert.Toast? CurrentToastMessage { get; set; }


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

            if (this.CurrentToastMessage == null)
                if (this.Queue.TryDequeue(out MetaFrm.Alert.Toast? toast) && toast != null)
                {
                    this.CurrentToastMessage = toast;
                    this.RunTimer();

                    this.InvokeAsync(this.StateHasChanged);
                }
        }

        /// <summary>
        /// SetParametersAsync
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            if (parameters.TryGetValue<MetaFrm.Alert.Toast>(nameof(ToastMessage), out var value))
            {
                if (value != null)
                    this.Queue.Enqueue(value);
            }

            await base.SetParametersAsync(parameters);
        }

        /// <summary>
        /// RunTimer
        /// </summary>
        public void RunTimer()
        {
            if (this.CurrentToastMessage != null && this.CurrentToastMessage.IsVisible)
            {
                try
                {
                    Timer timer = new(new TimerCallback(TimerProc));

                    if (this.CurrentToastMessage.Duration == ToastDuration.Short)
                        timer.Change(3000, 0);
                    else
                        timer.Change(6000, 0);
                }
                catch (Exception)
                {
                }
            }
        }
        private void TimerProc(object? state)
        {
            try
            {
                if (this.CurrentToastMessage != null && this.CurrentToastMessage.IsVisible)
                {
                    if (this.Queue.TryDequeue(out MetaFrm.Alert.Toast? toast) && toast != null)
                    {
                        this.CurrentToastMessage = toast;
                        this.RunTimer();
                    }
                    else
                        this.CurrentToastMessage = null;

                    this.InvokeAsync(this.StateHasChanged);
                }
            }
            catch (Exception)
            {
            }
        }

        private void Close()
        {
            if (this.CurrentToastMessage != null)
                this.CurrentToastMessage = null;
        }
    }
}