using System;
using UnityEngine;

public class UIEndTimeTimer : MonoBehaviour
    { 
        [SerializeField]
        RectTransform dayPanel;

        [SerializeField]
        RectTransform hourPanel;

        [SerializeField]
        RectTransform minutesPanel;

        [SerializeField]
        RectTransform secondPanel;

        [SerializeField]
        TMProLocalizer day;

        [SerializeField]
        TMProLocalizer hour;

        [SerializeField]
        TMProLocalizer minutes;

        [SerializeField]
        TMProLocalizer seconds;

        [SerializeField]
        bool stopOnZero = true;
        
        public DateTime endTime = DateTime.MinValue;
        
        long lastUpdateTime = -1L;

        void Start()
        {
            Update();
        }

        void Update()
        {
            var t = (long) Time.time;
            if (lastUpdateTime == t)
                return;

            lastUpdateTime = t;

            UpdateTime();
        }

        public void UpdateTime()
        {
            var timeNow = DateTime.Now;

            var time = endTime - timeNow;
            
            if (stopOnZero)
            {
                if (time <= TimeSpan.Zero)
                    time = TimeSpan.Zero;
            }
            
            dayPanel.gameObject.SetActive(time.Days > 0);
            hourPanel.gameObject.SetActive(time.Hours > 0 || time.Days > 0);
            minutesPanel.gameObject.SetActive(!(time.Days > 0));
            secondPanel.gameObject.SetActive(!(time.Hours > 0 || time.Days > 0));

            day.Localize(time.Days > 0 ? time.Days.ToString("00") : "00");
            hour.Localize(time.Hours > 0 ? time.Hours.ToString("00") : "00");
            minutes.Localize(time.Minutes > 0 ? time.Minutes.ToString("00") : "00");
            seconds.Localize(time.Seconds > 0 ? time.Seconds.ToString("00") : "00");
        }
    }
