using MetaFrm.Ads;
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
        private string? CurrentToastTimeSpanAgoString
        {
            get
            {
                if ( this.CurrentToastMessage == null)
                    return null;

                TimeSpan timeSpan = (DateTime)this.CurrentToastMessage.DateTimeNow - this.CurrentToastMessage.DateTime;

                if (timeSpan.TotalSeconds < 60)
                {
                    return this.Localization["{0}초 전", timeSpan.Seconds];
                }
                else if (timeSpan.TotalMinutes < 60)
                {
                    return this.Localization["{0}분 {1}초 전", timeSpan.Minutes, timeSpan.Seconds];
                }
                else if (timeSpan.TotalHours < 24)
                {
                    return $"{timeSpan.Hours}시 {timeSpan.Minutes}분 ago";
                }
                else if (timeSpan.TotalDays < 30)
                {
                    return $"{timeSpan.Days}일 {timeSpan.Hours}시간 ago";
                }
                else
                    return $"{this.CurrentToastMessage.DateTime:dd HH:mm:ss}";
            }
        }

        [Inject]
        private Maui.Ads.IAds? Ads { get; set; }

        private static string? AdsBannerClass { get; set; }

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
        /// OnInitialized
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (AdsBannerClass == null)
            {
                if (this.Ads != null && this.Ads is not DummyAds && !string.IsNullOrEmpty(this.Ads.BannerAdsId))
                    AdsBannerClass = this.GetAttribute(nameof(AdsBannerClass));
                else
                    AdsBannerClass = "";
            }
        }
        /// <summary>
        /// OnAfterRender
        /// </summary>
        /// <param name="firstRender"></param>
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (firstRender)
            { }

            if (this.CurrentToastMessage == null || (!this.CurrentToastMessage.IsVisible))
                if (this.Queue.TryDequeue(out MetaFrm.Alert.Toast? toast) && toast != null)
                {
                    this.CurrentToastMessage = toast;
                    this.RunTimer(toast);

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
                {
                    if (this.Queue.Count >= 128)
                        this.Queue.TryDequeue(out MetaFrm.Alert.Toast? _);

                    if (!this.Queue.Contains(value))
                        this.Queue.Enqueue(value);
                }
            }

            await base.SetParametersAsync(parameters);
        }

        /// <summary>
        /// RunTimer
        /// </summary>
        public void RunTimer(MetaFrm.Alert.Toast? toast)
        {
            if (toast != null)
            {
                try
                {
                    if (toast.Duration == ToastDuration.Short)
                    {
                        TimerToast timerToast = new()
                        {
                            Toast = toast,
                        };

                        timerToast.Timer = new(new TimerCallback(TimerProc), timerToast, 1500, 0);
                    }
                    else
                    {
                        TimerToast timerToast = new()
                        {
                            Toast = toast,
                        };

                        timerToast.Timer = new(new TimerCallback(TimerProc), timerToast, 3000, 0);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        private void TimerProc(object? state)
        {
            if (state != null && state is TimerToast item && this.CurrentToastMessage != null && this.CurrentToastMessage.Equals(item.Toast))
            {
                try
                {

                    if (this.Queue.TryDequeue(out MetaFrm.Alert.Toast? toast) && toast != null)
                    {
                        this.CurrentToastMessage = toast;
                        this.RunTimer(toast);
                    }
                    else
                    {
                        item.Toast?.IsVisible = false;
                    }

                    this.InvokeAsync(this.StateHasChanged);

                }
                catch (Exception)
                {
                }
                finally
                {
                    item.Timer?.Dispose();
                }
            }
        }

        private void Close()
        {
            if (this.CurrentToastMessage != null)
            {
                if (this.Queue.TryDequeue(out MetaFrm.Alert.Toast? toast) && toast != null)
                {
                    this.CurrentToastMessage = toast;
                    this.RunTimer(toast);
                }
                else
                    this.CurrentToastMessage.IsVisible = false;
            }
        }

        internal class TimerToast
        {
            public Timer? Timer { get; set; }
            public MetaFrm.Alert.Toast? Toast { get; set; }
        }
    }
}