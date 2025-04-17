using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppCharacter.Customization;
using UnityEngine;

namespace UberSideJobMod
{
    public enum Gender
    {
        Male,
        Female
    }
    public class UberPassenger
    {
        public int collisionCount { get; set; } = 0;

        public float pickupTime = -1f;
        public Address pickupAddress { get; set; }
        public Address dropoffAddress { get; set; }
        public Vector3 pickupLocation { get; set; }
        public Vector3 dropoffLocation { get; set; }
        public float fare { get; set; }
        public bool isPickedUp { get; set; }
        public float distanceToDestination { get; set; }
        public PassengerType passengerType { get; set; }
        public string passengerName { get; set; }

        public Gender gender;
        public float waitTime { get; set; } = 0f;
        public bool isCancelled { get; set; } = false;
        public string[] conversationLines { get; set; }
        public string[] maleConversationLines;
        public string[] femaleConversationLines;
        public int suddenStopCount { get; set; } = 0;
        public float totalSpeedingTime { get; set; } = 0f;
        public float tipAmount { get; set; } = 0f;
        public bool tipCalculated { get; set; } = false;
        public int positiveInteractions { get; set; } = 0;
        public int negativeInteractions { get; set; } = 0;
        public bool hadSpecialConversation { get; set; } = false;
        public int dynamicLineCount;
    }
}
