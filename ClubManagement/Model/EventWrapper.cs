using Google.Apis.Calendar.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClubManagement.Model
{
    public class EventWrapper
    {
        public EventWrapper(LocalEvent localEvent)
        {
            LocalEvent = localEvent;
            IsLocal = true;
        }

        public EventWrapper(Event googleEvent)
        {
            GoogleEvent = googleEvent;
            IsLocal = false;
        }

        public LocalEvent LocalEvent { get; }
        public Event GoogleEvent { get; }
        public bool IsLocal { get; }

        public override string ToString()
        {
            if (IsLocal)
            {
                return $"{LocalEvent.Summary} ({LocalEvent.Start.ToString("g")})";
            }
            else
            {
                return $"{GoogleEvent.Summary} ({GoogleEvent.Start.DateTime?.ToString("g") ?? GoogleEvent.Start.Date})";
            }
        }
    }

}
