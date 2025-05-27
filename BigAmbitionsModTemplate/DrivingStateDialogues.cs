using System;
using System.Collections.Generic;

namespace RideshareSideJobMod
{
    public class DrivingStateDialogues
    {
        public readonly Dictionary<PassengerType, Dictionary<string, List<string>>> NotificationMessages = new Dictionary<PassengerType, Dictionary<string, List<string>>>
        {
            [PassengerType.Business] = new Dictionary<string, List<string>>
            {
                ["speeding"] = new List<string>
                {
                    "Hey {playerName}, I'm on a tight schedule—can you slow down?",
                    "Excuse me {playerName}, this speed is unprofessional, please ease up!",
                    "Hey {playerName}, slow down, I'd like to arrive in one piece!",
                    "Attention {playerName}, let's keep it professional—reduce speed, please!",
                    "Hello {playerName}, I have a call soon—can we slow down a bit?",
                    "Hi {playerName}, could we maintain a more reasonable speed? My laptop is sliding!",
                    "Pardon me {playerName}, this pace is concerning for my upcoming presentation!",
                    "Excuse me {playerName}, I need to review documents—this speed is making it impossible!",
                    "Hey {playerName}, my colleagues would appreciate me arriving safely—please slow down!",
                    "Listen {playerName}, I'm trying to prepare for a meeting—can we drive more cautiously?",
                    "Hey {playerName}, this velocity is disrupting my conference call—slow it down!",
                    "Excuse me {playerName}, my briefcase is about to topple—ease off the gas!",
                    "Hi {playerName}, a calmer pace would help me finish these emails, please!",
                    "Hey {playerName}, I’m on a deadline—can you prioritize a steady speed?",
                    "Pardon {playerName}, this speed could jeopardize my next appointment—slow down!",
                    "Hello {playerName}, my files are sliding—let’s take it easier, shall we?",
                    "Hey {playerName}, I need to look professional—can we avoid this rush?",
                    "Excuse me {playerName}, my notes are a mess at this speed—please slow!",
                    "Hi {playerName}, a slower drive would let me prep for my pitch—thanks!",
                    "Hey {playerName}, let’s keep it safe and slow for my important meeting!"
                },
                ["sudden_stop"] = new List<string>
                {
                    "Hey {playerName}, that stop was abrupt—fortunately, I'm belted in!",
                    "Goodness {playerName}, please, no more sudden stops, I have a meeting!",
                    "Whoa {playerName}, a smoother ride would be much appreciated!",
                    "Hey {playerName}, that jolt nearly cost me my notes—easy on the brakes!",
                    "Look {playerName}, let's avoid those stops; I'm on a deadline!",
                    "Hey {playerName}, that brake check nearly spilled my coffee on my laptop!",
                    "Excuse me {playerName}, sudden stops like that are affecting my ability to work!",
                    "Hi {playerName}, could we avoid dramatic stops? My presentation materials are flying!",
                    "Please {playerName}, gentler braking would help me maintain my professional appearance!",
                    "Hey {playerName}, I need to arrive composed for my meeting—these stops aren't helping!",
                    "Whoa {playerName}, that stop almost knocked my tablet off—gentler, please!",
                    "Hey {playerName}, my coffee mug tipped—can we brake more smoothly?",
                    "Excuse me {playerName}, that jolt disrupted my call—let’s ease up!",
                    "Hi {playerName}, my documents scattered with that stop—smoother next time!",
                    "Hey {playerName}, I nearly dropped my phone—please avoid harsh stops!",
                    "Pardon {playerName}, that brake action messed up my schedule—be gentle!",
                    "Hello {playerName}, my briefcase flew open—let’s try softer braking!",
                    "Hey {playerName}, those stops are stressing me out—can we smooth it out?",
                    "Excuse me {playerName}, I lost my place in my report—easier stops, please!",
                    "Hi {playerName}, a gentle stop would keep my work intact—thanks!"
                }
            },
            [PassengerType.Tourist] = new Dictionary<string, List<string>>
            {
                ["speeding"] = new List<string>
                {
                    "Hey {playerName}, whoa, slow down—I want to enjoy the sights!",
                    "Excuse me {playerName}, can you ease up? I'm here to explore, not race!",
                    "Hello {playerName}, slow down, I'd love to take some photos!",
                    "Hi {playerName}, let's take it slow—I'm soaking in the views!",
                    "Hey {playerName}, too fast! I missed that landmark!",
                    "Wow {playerName}, we're zooming past all the attractions I wanted to see!",
                    "Hello there {playerName}, my guidebook mentioned scenic views, but they're just blurs at this speed!",
                    "Hey {playerName}, I paid for the scenic route, not the express lane!",
                    "Excuse me {playerName}, my vacation photos will be nothing but streaks at this pace!",
                    "Hi {playerName}, I'm trying to spot local wildlife—impossible at this velocity!",
                    "Hey {playerName}, slow down—I need to capture that beautiful view!",
                    "Whoa {playerName}, we’re missing the architecture at this speed!",
                    "Hello {playerName}, my map can’t keep up—let’s slow down!",
                    "Hey {playerName}, I want to savor the culture, not speed through it!",
                    "Excuse me {playerName}, this pace is ruining my travel journal!",
                    "Hi {playerName}, the coastline’s a blur—can we take it easier?",
                    "Hey {playerName}, I’m here for the experience—slow down, please!",
                    "Whoa {playerName}, my binoculars can’t track anything at this speed!",
                    "Hello {playerName}, let’s enjoy the journey—ease up on the gas!",
                    "Hey {playerName}, I’m missing the street performers—slow it down!"
                },
                ["sudden_stop"] = new List<string>
                {
                    "Hey {playerName}, that stop startled me—good thing I'm buckled up!",
                    "Goodness {playerName}, oh my, let's avoid those sudden stops, okay?",
                    "Whoa {playerName}, that was a jolt—let's keep it smooth, please!",
                    "Hey {playerName}, my camera almost flew—smoother stops, please!",
                    "Oh {playerName}, that stop shook my map—easy next time!",
                    "Hey {playerName}, my souvenir snow globe almost shattered with that stop!",
                    "Excuse me {playerName}, these sudden stops are ruining my vacation videos!",
                    "Hi {playerName}, I'd like to remember this trip fondly—not for the whiplash!",
                    "Whoa {playerName}, I nearly lost my guidebook with that stop—can we be gentler?",
                    "Hey {playerName}, my postcards went flying! Let's have a smoother journey!",
                    "Hey {playerName}, that stop jolted my camera lens—please be smoother!",
                    "Whoa {playerName}, my travel snacks spilled—gentler braking next time!",
                    "Excuse me {playerName}, that stop shook my souvenirs—let’s ease up!",
                    "Hi {playerName}, my itinerary flew off—smoother stops, please!",
                    "Hey {playerName}, that jolt nearly dropped my sunglasses—be careful!",
                    "Oh {playerName}, my travel diary pages flipped—gentler stops!",
                    "Hey {playerName}, that stop messed up my photo framing—smoother, please!",
                    "Whoa {playerName}, my water bottle tipped—let’s avoid those jolts!",
                    "Hello {playerName}, that stop startled my travel buddy—easier braking!",
                    "Hey {playerName}, my hat flew off—can we keep it smooth from now on?"
                }
            },
            [PassengerType.Party] = new Dictionary<string, List<string>>
            {
                ["speeding"] = new List<string>
                {
                    "Yo {playerName}, slow down will ya? sorry.", // [HOTFIX] out of context if this is from business to residential.
                    "Hey {playerName}, chill on the speed, let's keep the vibes cool!",
                    "Whoa {playerName}, ease up, I don't wanna spill my drink!",
                    "Whoa {playerName}, slow it down—the party's at the destination!",
                    "Hey {playerName}, easy—save the rush for the dance floor!",
                    "Whoa {playerName}, we wanna arrive alive for the afterparty!",
                    "Listen {playerName}, this speed is killing my pre-game mood!",
                    "Hey {playerName}, my playlist hasn't even finished—why the rush?",
                    "Yo {playerName}, slow your roll—we're trying to take selfies back here!",
                    "Hey {playerName}, ease up—my hair's gonna be a mess before we even arrive!",
                    "Whoa {playerName}, slow down—my party snacks are sliding!",
                    "Yo {playerName}, this speed’s messing with my dance moves prep!",
                    "Hey {playerName}, let’s cruise—save the speed for the DJ!",
                    "Whoa {playerName}, my glow sticks are flying—ease up!",
                    "Hey {playerName}, slow it down—we need to hype up first!",
                    "Yo {playerName}, this rush is killing my party playlist flow!",
                    "Hey {playerName}, my drink’s sloshing—let’s take it chill!",
                    "Whoa {playerName}, we’re missing the pre-party vibe—slow down!",
                    "Hey {playerName}, ease off—the party’s better at a steady pace!",
                    "Yo {playerName}, my sunglasses are slipping—slow your roll!"
                },
                ["sudden_stop"] = new List<string>
                {
                    "Yo {playerName}, whoa, that stop—luckily I'm strapped in!",
                    "Hey {playerName}, easy on the brakes, let's keep the party going!",
                    "Whoa {playerName}, that stop killed the vibe—smoother, please!",
                    "Whoa {playerName}, my drink almost spilled—gentle stops, man!",
                    "Hey {playerName}, that brake slam was a buzzkill—take it easy!",
                    "Whoa {playerName}, I nearly face-planted the seat with that stop!",
                    "Hey {playerName}, my party outfit's getting wrinkled with these jolts!",
                    "Yo {playerName}, careful! My phone almost launched into orbit!",
                    "Whoa {playerName}, smooth driving equals better tips, you know?",
                    "Hey {playerName}, ease up on those brakes—my makeup's gonna smudge!",
                    "Yo {playerName}, that stop spilled my cocktail—smoother next time!",
                    "Hey {playerName}, my party hat flew off—gentle braking, please!",
                    "Whoa {playerName}, that jolt messed up my dance pose—ease up!",
                    "Hey {playerName}, my speakers skipped—smooth stops, man!",
                    "Yo {playerName}, that stop killed my hype—let’s keep it chill!",
                    "Hey {playerName}, my party snacks scattered—gentler brakes!",
                    "Whoa {playerName}, my glow stick broke—careful next time!",
                    "Hey {playerName}, that stop threw my vibe off—smoother, please!",
                    "Yo {playerName}, my phone case cracked—easy on the brakes!",
                    "Hey {playerName}, let’s keep the party rolling—gentle stops only!"
                }
            },
            [PassengerType.Silent] = new Dictionary<string, List<string>>
            {
                ["speeding"] = new List<string>
                {
                    "Um, {playerName}, *sighs* Can you slow down?",
                    "Hey {playerName}, *frowns* Please, less speed.",
                    "{playerName}, *quietly* Slow down, okay?",
                    "Excuse me {playerName}, *mutters* Too fast… ease off.",
                    "Hi {playerName}, *soft glare* Slow it down, please.",
                    "Hey {playerName}, *taps nervously* Speed... concerning.",
                    "{playerName}, *gestures downward* Slower... please.",
                    "Um, {playerName}, *points at speedometer* That's... fast.",
                    "Hey {playerName}, *adjusts glasses nervously* Slower would be... better.",
                    "{playerName}, *brief eye contact* Too fast for comfort.",
                    "Hey {playerName}, *shifts slightly* Speed... unsettling.",
                    "Um, {playerName}, *quiet nod* Slow down... if you can.",
                    "{playerName}, *fidgets* This pace... too much.",
                    "Hey {playerName}, *low voice* Ease off... please.",
                    "Excuse me {playerName}, *subtle shake of head* Too fast.",
                    "{playerName}, *glances out window* Slower... preferred.",
                    "Hey {playerName}, *tightens grip* Speed... worrying.",
                    "Um, {playerName}, *clears throat* Can it... slow?",
                    "{playerName}, *small sigh* Less speed... better.",
                    "Hey {playerName}, *tilts head* Slow down... quietly."
                },
                ["sudden_stop"] = new List<string>
                {
                    "Hey {playerName}, *grips seat* I'm belted, but still…",
                    "{playerName}, *mutters* That stop was harsh.",
                    "Um, {playerName}, *silent glare* Smoother, please.",
                    "Hey {playerName}, *shifts uncomfortably* That was rough.",
                    "{playerName}, *quiet huff* Easy on the brakes.",
                    "Hey {playerName}, *steadies glasses* That... startled me.",
                    "{playerName}, *exhales slowly* Gentler... preferred.",
                    "Um, {playerName}, *clutches bag tighter* That was... abrupt.",
                    "Hey {playerName}, *recovers posture* Careful with stops.",
                    "{playerName}, *straightens jacket* Smoother... technique... please.",
                    "Hey {playerName}, *adjusts position* That stop... jarring.",
                    "Um, {playerName}, *tightens belt* Too sudden...",
                    "{playerName}, *quiet wince* Easier braking... please.",
                    "Hey {playerName}, *shifts bag* That was... unsettling.",
                    "Excuse me {playerName}, *small flinch* Gentler stops...",
                    "{playerName}, *nods slowly* Smoother... better.",
                    "Hey {playerName}, *grips armrest* That jolt... unexpected.",
                    "Um, {playerName}, *clears throat quietly* Softer... stops.",
                    "{playerName}, *adjusts hat* That was... rough.",
                    "Hey {playerName}, *slight frown* Easier on the brakes... thanks."
                }
            },
            [PassengerType.Regular] = new Dictionary<string, List<string>>
            {
                ["speeding"] = new List<string>
                {
                    "Hey {playerName}, can you slow down a bit, please?",
                    "Excuse me {playerName}, let's take it easy on the speed, okay?",
                    "Hi {playerName}, slow down, I'm not in a rush!",
                    "Hello {playerName}, please ease up on the gas a little!",
                    "Hey {playerName}, a slower pace would suit me fine!",
                    "Um, {playerName}, this speed is making me a bit nervous!",
                    "Excuse me {playerName}, could we drive a little more carefully?",
                    "Hi {playerName}, I'd feel safer if we slowed down some!",
                    "Hey {playerName}, no need to hurry on my account—take it easy!",
                    "Hello there {playerName}, mind slowing down? This isn't a race!",
                    "Hey {playerName}, this pace is a bit fast for my liking—can we slow?",
                    "Excuse me {playerName}, a calmer drive would be nice, please!",
                    "Hi {playerName}, I’d prefer a steadier speed if that’s okay!",
                    "Hey {playerName}, let’s not rush—slow down a bit, please!",
                    "Hello {playerName}, my nerves would thank you for a slower ride!",
                    "Hey {playerName}, this speed feels a tad too quick—ease up?",
                    "Excuse me {playerName}, could we take it slower for my comfort?",
                    "Hi {playerName}, I’d appreciate a more relaxed pace, if possible!",
                    "Hey {playerName}, no hurry here—let’s drive a bit slower!",
                    "Hello {playerName}, a gentler speed would make this trip nicer!"
                },
                ["sudden_stop"] = new List<string>
                {
                    "Hey {playerName}, that stop was rough—good thing I'm buckled!",
                    "Whoa {playerName}, let's keep it smooth, that stop was jarring!",
                    "Oh {playerName}, easy on the brakes, please!",
                    "Hey {playerName}, that jolt surprised me—smoother next time!",
                    "Excuse me {playerName}, please avoid those sudden stops!",
                    "Goodness {playerName}, that stop nearly gave me whiplash!",
                    "Hey {playerName}, gentler braking would be much appreciated!",
                    "Whoa {playerName}, I almost hit my head on that stop!",
                    "Hi {playerName}, could we avoid the roller-coaster braking?",
                    "Hey {playerName}, smooth driving makes for a better experience for both of us!",
                    "Hey {playerName}, that stop shook my bag—please be gentler!",
                    "Whoa {playerName}, I nearly dropped my phone with that brake!",
                    "Excuse me {playerName}, that jolt was a surprise—smoother, please!",
                    "Hi {playerName}, my coffee cup tipped—easier stops next time!",
                    "Hey {playerName}, that stop felt like a bump ride—gentle down!",
                    "Oh {playerName}, my glasses slid off—can we brake softer?",
                    "Hey {playerName}, that stop caught me off guard—smoother, please!",
                    "Excuse me {playerName}, my book fell—let’s avoid those jolts!",
                    "Hi {playerName}, a gentler stop would keep my belongings safe!",
                    "Hey {playerName}, that brake action startled me—ease up next time!"
                }
            }
        };

        public string GetRandomMessage(PassengerType passengerType, string eventType)
        {
            if (!NotificationMessages.ContainsKey(passengerType) || !NotificationMessages[passengerType].ContainsKey(eventType))
            {
                return string.Empty;
            }
            var messages = NotificationMessages[passengerType][eventType];
            string key = $"driving_{passengerType}_{eventType}";
            return messages[ConversationTracker.GetRandomLineIndex(messages, key)];
        }
    }
}