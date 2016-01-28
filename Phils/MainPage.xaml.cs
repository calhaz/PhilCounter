using System;
using System.Threading;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using DayOfWeek = System.DayOfWeek;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Phils
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private string _antalMinuterTillPhils = "Nästa Phils:";
        private string _antalMinuterTillRsfDaily = "Nästa RSF daily standup:";
        private string _antalMinuterTillPrioDaily = "Nästa PRIO daily standup:";
        private readonly Timer m_timer;
        private readonly CoreDispatcher disp;
        private int m_lastValue;
        private readonly int _dueTime;
        private bool _shoutPrioMin;
        private bool _shoutPrioSec;

        private DayOfWeek _philDay = DayOfWeek.Friday;

        private int _RsfHour = 09;
        private int _RsfMinutes = 30;

        private int _PrioHour = 11;
        private int _PrioMinutes = 00;

        private int _PhilsHour = 10;
        private int _PhilsMinutes = 45;

        public MainPage()
        {
            InitializeComponent();

            disp = Window.Current.Dispatcher;

            _dueTime = 1000;

            m_timer = new Timer(Callback, null, 100, Timeout.Infinite);
        }

        private async void Callback(object state)
        {
            await disp.RunAsync(CoreDispatcherPriority.Normal, Update);
            m_timer.Change(_dueTime, Timeout.Infinite);
        }

        private void Update()
        {
            m_textBoxTime.Text = SelectCountDown();
        }

        private string SelectCountDown()
        {
            //If clock is before 9:30, next item is RSF daily
            var now = DateTime.Now;
            if (now.Hour <= _RsfHour || now.Hour > _PhilsHour)
            {
                if (now.Hour == _RsfHour && now.Minute > _RsfMinutes)
                    return MinutesLeftToPrio(_antalMinuterTillPrioDaily, _PrioHour, _PrioMinutes, "PRIO daily standup", now.Day);

                return MinutesLeftToPrio(_antalMinuterTillRsfDaily, _RsfHour, _RsfMinutes, "RSF daily standup", now.Day);
            }

            //If clock is before 10:00, next item is PRIO daily
            if (now.Hour <= _PrioHour)
            {
                //if(now.Hour <= _PrioHour && now.Minute < _PrioMinutes)
                    return MinutesLeftToPrio(_antalMinuterTillPrioDaily, _PrioHour, _PrioMinutes, "PRIO daily standup", now.Day);
            }

            //Else if clock i past 10:00 on a friday, next item is Phils
            return MinutesLeftToPrio(_antalMinuterTillPhils, _PhilsHour, _PhilsMinutes, "Phils", NextFriday(), "GO GO GO to Phils and eat lots of hamburgers");
        }

        private string MinutesLeftToPrio(string headerText, int hour, int minutes, string action, int day, string extraAction = null)
        {
            m_textBoxName.Text = headerText;
            var now = DateTime.Now;

            var nextTime = DateTime.Parse($"{now.Year}-{now.Month}-{day} {hour}:{minutes}:00");

            var timeLeft = nextTime - now;

            if (timeLeft.Hours == 0 && now.Day == nextTime.Day && now.DayOfWeek != DayOfWeek.Saturday && now.DayOfWeek != DayOfWeek.Sunday)
            {
                m_lastValue = timeLeft.Minutes + 1;
                if (m_lastValue > 3 && timeLeft.Hours == 0)
                    return m_lastValue.ToString("##") + " minuter";

                if (m_lastValue > 1)
                {
                    if (!_shoutPrioMin)
                    {
                        _shoutPrioMin = true;
                        ShoutOut(m_lastValue + " minutes to " + action);
                    }
                    return m_lastValue.ToString("##") + " minuter";
                }

                if (timeLeft.TotalSeconds > 1 && timeLeft.TotalSeconds <= 30)
                {
                    if (!_shoutPrioSec)
                    {
                        _shoutPrioSec = true;
                        ShoutOut($"{timeLeft.Seconds} seconds to " + action);
                    }
                    if (timeLeft.TotalSeconds <= 10)
                        ShoutOut($"{timeLeft.Seconds}");
                    return timeLeft.Seconds.ToString("##") + " sekunder";
                }

                if ((int)timeLeft.TotalSeconds == 0)
                {
                    ShoutOut(string.Format("{0}{1}", $"Now it´s time for {action}", extraAction ?? ""));
                    _shoutPrioMin = false;
                    _shoutPrioSec = false;
                }

                return m_lastValue.ToString("##") + " minuter";
            }

            if(timeLeft.Days <= 0 && timeLeft.Hours <= 0 && timeLeft.Minutes <= 0)
                return $"{NextDay(nextTime)} / {nextTime.Month} - kl: {nextTime.Hour}:{nextTime.Minute}";

            return $"{timeLeft.Days} dagar {timeLeft.Hours} timmar {timeLeft.Minutes} minuter";
        }

        private string NextDay(DateTime nextTime)
        {
            if(nextTime.DayOfWeek == DayOfWeek.Friday)
                return $"{nextTime.Day + 3}";
            if (nextTime.DayOfWeek == DayOfWeek.Saturday)
                return $"{nextTime.Day + 2}";

            return $"{nextTime.Day + 1}";
        }

        private int NextFriday()
        {
            //For testing
            if (_philDay == DateTime.Now.DayOfWeek)
                return DateTime.Now.Day;


            if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                return DateTime.Now.Day + 4;
            if (DateTime.Now.DayOfWeek == DayOfWeek.Tuesday)
                return DateTime.Now.Day + 3;
            if (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday)
                return DateTime.Now.Day + 2;
            if (DateTime.Now.DayOfWeek == DayOfWeek.Thursday)
                return DateTime.Now.Day + 1;
            if (DateTime.Now.DayOfWeek == DayOfWeek.Friday)
                return DateTime.Now.Day;
            if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                return DateTime.Now.Day + 6;
            if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                return DateTime.Now.Day + 5;

            return -1;
        }

        private async void ShoutOut(string text)
        {
            var mediaElement = new MediaElement();
            var speech = new SpeechSynthesizer();
            var stream = await speech.SynthesizeTextToStreamAsync(text);
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
        }
    }
}