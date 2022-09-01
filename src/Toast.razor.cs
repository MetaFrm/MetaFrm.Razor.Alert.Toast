﻿using MetaFrm.Alert;
using Microsoft.AspNetCore.Components;

namespace MetaFrm.Razor.Alert
{
    /// <summary>
    /// Toast
    /// </summary>
    public partial class Toast
    {
        private int runCount = 0;

        /// <summary>
        /// ToastMessage
        /// </summary>
        [Parameter]
        public MetaFrm.Alert.Toast? ToastMessage { get; set; }

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

                    this.InvokeStateHasChanged();
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