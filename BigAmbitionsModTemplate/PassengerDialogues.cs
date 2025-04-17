using System.Collections.Generic;
using Il2CppEnums;
using MelonLoader;
using System.Linq;

namespace UberSideJobMod
{
    public static class PassengerDialogues
    {
        public static string[] MaleFirstNames = {
            "James", "John", "Robert", "Michael", "William", "David", "Richard", "Joseph", "Thomas",
            "Charles", "Christopher", "Daniel", "Matthew", "Anthony", "Donald", "Mark", "Paul", "Steven",
            "Andrew", "Kenneth", "George", "Edward", "Brian", "Ronald", "Timothy", "Jason", "Jeffrey",
            "Benjamin", "Samuel", "Joshua", "Kevin", "Eric", "Stephen", "Gregory", "Harold", "Patrick",
            "Raymond", "Jacob", "Henry", "Jonathan", "Justin", "Scott", "Brandon", "Peter", "Frank",
            "Nicholas", "Dennis", "Zachary", "Ethan", "Kyle", "Jerry", "Aaron", "Walter", "Adam",
            "Lawrence", "Ryan", "Gary", "Jeremy", "Louis", "Randy", "Howard", "Eugene", "Vincent"
        };

        public static string[] FemaleFirstNames = {
            "Sarah", "Emma", "Olivia", "Ava", "Isabella", "Sophia", "Charlotte", "Mia", "Amelia", "Harper",
            "Evelyn", "Abigail", "Emily", "Elizabeth", "Sofia", "Madison", "Scarlett", "Victoria", "Grace",
            "Lily", "Hannah", "Julia", "Natalie", "Chloe", "Zoey", "Penelope", "Layla",
            "Aurora", "Violet", "Eleanor", "Alice", "Samantha", "Ella", "Zoe", "Claire", "Audrey",
            "Luna", "Stella", "Hazel", "Nora", "Lucy", "Mila", "Aubrey", "Lillian", "Ellie", "Bella",
            "Leah", "Anna", "Aria", "Caroline", "Riley", "Savannah", "Madelyn", "Maya", "Addison",
            "Skylar", "Piper", "Paisley", "Ruby", "Eva", "Naomi", "Autumn"
        };

        public static string[] LastNames = {
            "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor",
            "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson", "Garcia", "Martinez",
            "Robinson", "Clark", "Rodriguez", "Lewis", "Lee", "Walker", "Hall", "Allen", "Young", "King",
            "Wright", "Scott", "Green", "Adams", "Baker", "Nelson", "Carter", "Mitchell", "Perez", "Roberts",
            "Hernandez", "Turner", "Parker", "Collins", "Sanchez", "Morris", "Rogers", "Reed", "Cook",
            "Morgan", "Bell", "Murphy", "Bailey", "Rivera", "Cooper", "Richardson", "Cox", "Howard",
            "Ward", "Torres", "Peterson", "Gray", "Ramirez", "James", "Watson", "Brooks", "Kelly",
            "Sanders", "Price", "Bennett", "Wood", "Barnes", "Ross", "Henderson", "Coleman", "Jenkins"
        };

        public static Dictionary<PassengerType, Dictionary<string, Dictionary<Gender, List<string>>>> Comments = new Dictionary<PassengerType, Dictionary<string, Dictionary<Gender, List<string>>>>
        {
            { PassengerType.Regular, new Dictionary<string, Dictionary<Gender, List<string>>>
                {
                    {"regular", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Hey, man, how’s your day going?",
                                "Good to see a fellow gent driving!",
                                "Been busy today, dude?",
                                "Nice weather we’re having, right?",
                                "Thanks for the ride, bro!",
                                "I take Uber every day—good gig, man?",
                                "Just heading home from work, same as you?",
                                "You been driving long today, pal?",
                                "Traffic’s wild around here, huh?",
                                "You ever had any big shots in your car?",
                                "This city’s got some crazy energy, man!",
                                "Just running errands—boring stuff, dude.",
                                "Know a good coffee spot, my man?",
                                "My last driver got lost—you seem solid!",
                                "Traffic’s a mess, but you’re handling it.",
                                "I’m late for dinner—!",
                                "Those food trucks downtown? Fire, man.",
                                "This car’s comfier than my recliner!"
                            }},
                            { Gender.Female, new List<string> {
                                "Hi, how’s your day going?",
                                "Nice to have a lady behind the wheel!",
                                "You keeping busy today?",
                                "Isn’t the weather just lovely?",
                                "Thanks for the ride!",
                                "I’m an Uber regular—you enjoy driving?",
                                "Heading home from work—long day?",
                                "You been out here driving all day?",
                                "Traffic’s always nuts here, right?",
                                "Ever pick up anyone famous?",
                                "This city’s got such a cool vibe!",
                                "Just doing errands—nothing fun.",
                                "Any favorite coffee places nearby?",
                                "My last driver was hopeless—you’re great!",
                                "Traffic’s awful, but you’re a pro.",
                                "I’m late for dinner—hope it’s okay!",
                                "You tried those downtown food trucks?",
                                "This car’s so cozy, I love it!"
                            }}
                        }},
                    {"dynamic", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Just came from the {0}, now heading to the {1}, man.",
                                "Picked me up at the {0}, gotta get to the {1}—you good, dude?",
                                "I was at the {0}, now off to the {1}—how’s the drive, bro?",
                                "Leaving the {0} to check out the {1}—any shortcuts, pal?"
                            }},
                            { Gender.Female, new List<string> {
                                "Just left the {0}, now going to the {1}, hon!",
                                "From the {0} to the {1}—hope it’s a smooth ride, love.",
                                "I was at the {0}, heading to the {1}—how’s traffic, dear?",
                                "Leaving the {0} to visit the {1}—know a quick route, sweetie?"
                            }}
                        }},
                    {"flirty", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Hey there, handsome, how’s your day?",
                                "Looking good behind the wheel, driver!",
                                "Got any plans after this ride, cutie?",
                                "You’re making my day better, stud!"
                            }},
                            { Gender.Female, new List<string> {
                                "Hi, love, how’s your day going?",
                                "You’re a charmer behind the wheel, dear!",
                                "Free after this ride, sweetie?",
                                "You’re brightening my day, gorgeous!"
                            }}
                        }},
                    {"speeding", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Whoa, man, can you slow it down?",
                                "We’re flying, dude—ease up!",
                                "Hey, bro, speed limit’s back there!",
                                "Not in a huge rush, pal!",
                                "Chill on the gas, my man!"
                            }},
                            { Gender.Female, new List<string> {
                                "Hey, hon, can we slow down a bit?",
                                "You’re speeding, love—take it easy!",
                                "Whoa, dear, let’s not race!",
                                "No need to hurry that much, sweetie!",
                                "Ease off the pedal, please!"
                            }}
                        }},
                    {"sudden_stop", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Oof, that was rough, man!",
                                "Easy on the brakes, dude!",
                                "My coffee’s everywhere, bro!",
                                "That stop shook me up, pal!",
                                "Warn me next time!"
                            }},
                            { Gender.Female, new List<string> {
                                "Oh, love, that was sudden!",
                                "Careful with the brakes, hon!",
                                "My drink almost spilled, dear!",
                                "That stop startled me, sweetie!",
                                "Give a heads-up next time, please!"
                            }}
                        }},
                    {"smooth", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Smooth ride, man, nice one!",
                                "You’re a solid driver, dude.",
                                "Could chill to this driving, bro!",
                                "Five stars for you, pal!",
                                "Gliding like a pro, my man."
                            }},
                            { Gender.Female, new List<string> {
                                "Such a smooth ride, love!",
                                "You’re a great driver, hon.",
                                "This driving’s so relaxing, dear!",
                                "You’re earning five stars, sweetie!",
                                "Cruising perfectly, aren’t you?"
                            }}
                        }},
                    {"collision", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Whoa, man, what’d we hit?!",
                                "You okay, dude? That was bad!",
                                "That’s gonna hurt your rating, bro!",
                                "Hope the car’s fine, pal!",
                                "Bumper cars, huh?"
                            }},
                            { Gender.Female, new List<string> {
                                "Oh no, love, what was that?!",
                                "You alright, hon? That was rough!",
                                "That crash isn’t good, dear!",
                                "Hope your car’s okay, sweetie!",
                                "Not a demolition derby, please!"
                            }}
                        }}
                }
            },
            // Other passenger types remain unchanged for brevity
            { PassengerType.Business, new Dictionary<string, Dictionary<Gender, List<string>>>
                {
                    {"regular", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Running late for a meeting, man.",
                                "Can we speed up? Got a call soon.",
                                "Know the fastest route, pal?",
                                "Traffic’s brutal—always is, huh?",
                                "Last driver was awful—better luck now.",
                                "This deal’s make-or-break, bro.",
                                "Side entrance, please—tight schedule.",
                                "Three meetings today, exhausting.",
                                "New office adds 20 minutes, ugh.",
                                "Gonna send emails—cool, dude?",
                                "Boss is on my case, as usual.",
                                "Corporate life’s rough, you know?",
                                "No time for coffee—sucks, man.",
                                "This deal’s huge—fingers crossed!",
                                "Pitching investors—big day, pal.",
                                "Any Midtown shortcuts, bro?",
                                "Assistant booked you—hope you’re good.",
                                "Last quarter tanked—here’s hoping."
                            }},
                            { Gender.Female, new List<string> {
                                "I’m late for a meeting.",
                                "Can we hurry? Conference call soon.",
                                "Fastest route, please?",
                                "Traffic never improves, does it?",
                                "My last driver was hopeless—trusting you.",
                                "This presentation’s critical.",
                                "Drop me at the side, please—running late.",
                                "Back-to-back meetings today, ugh.",
                                "New office is so far—hate it.",
                                "Mind if I send emails?",
                                "My boss won’t let up today.",
                                "Ever deal with execs? Tough, right?",
                                "Dying for coffee but no time.",
                                "Huge deal today—wish me luck!",
                                "Investor pitch—can’t be late.",
                                "Know shortcuts to Midtown?",
                                "My assistant picked you—don’t let me down.",
                                "Rough quarter—hoping for better."
                            }}
                        }},
                    {"dynamic_day", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Coming from the {0}, got a meeting at the {1}—can’t be late.",
                                "Left the {0}, need to hit the {1} for a deal.",
                                "From the {0} to the {1}—fastest route, pal?",
                                "Was at the {0}, heading to the {1} for a big pitch, bro."
                            }},
                            { Gender.Female, new List<string> {
                                "Left the {0}, need to reach the {1} for a deal.",
                                "From the {0} to the {1}—quick way, please?",
                                "Got a pitch at the {1}, was at the {0}—fastest route?",
                                "Coming from the {0}, off to the {1}—tight schedule."
                            }}
                        }},
                    {"dynamic_day_same_type", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Met a client at the {0}, now another meeting at the {0}—hurry!",
                                "Busy day at the {0}—another deal to close, pal.",
                                "From one {0} meeting to the next—keep it quick, dude!",
                                "Handling two cases at the {0} today—fastest route, bro!"
                            }},
                            { Gender.Female, new List<string> {
                                "Leaving the {0}, now another meeting at the {0}—hurry, please.",
                                "Met a client at the {0}, now another meeting at the {0}—hurry, hon!",
                                "Busy day at the {0}—another deal to close, dear.",
                                "From one {0} meeting to the next—keep it quick!",
                                "Handling two cases at the {0} today—fastest route, sweetie!"
                            }}
                        }},
                    {"dynamic_night", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Entertaining clients at the {0}, now to the {1}—keep it quick, man.",
                                "Late visit to the {0}, heading to the {1}—urgent stuff, dude.",
                                "Was at the {0}, need to check the {1}—no delays, bro.",
                                "From the {0} to the Residential for a late client call, pal.",
                                "Left the {0}, heading home to the Residential—urgent docs, dude.",
                                "Residential to the {1} after a long day—final emails, man.",
                                "From the Residential to the {1}—late-night prep, bro."
                            }},
                            { Gender.Female, new List<string> {
                                "Client drinks at the {0}, now to the {1}—hurry, ma'am!.",
                                "Checking the {0} late, off to the {1}—important, hon.",
                                "Left the {0}, heading to the {1}—can’t wait, sweetie.",
                                "From the {0} to the Residential for a late client call.",
                                "Left the {0}, heading home to the Residential—urgent docs, hon.",
                                "Residential to the {1} after a long day—final emails, dear.",
                                "From the Residential to the {1}—late-night prep, sweetie."
                            }}
                        }},
                    {"flirty", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Looking sharp, driver—big plans later?",
                                "Handsome and a good driver? Impressive!",
                                "You’re making this ride enjoyable, sir!"
                            }},
                            { Gender.Female, new List<string> {
                                "You look lovely today, driver!",
                                "Charming and skilled—nice combo, dear!",
                                "This ride’s better with you, sweetie!"
                            }}
                        }},
                    {"speeding", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Slow down, man, safety first.",
                                "Crashing’s worse than late, dude.",
                                "Rather be late than wrecked, bro.",
                                "This ain’t a race, pal.",
                                "Ease up—insurance won’t like this."
                            }},
                            { Gender.Female, new List<string> {
                                "Please slow down, dear, be safe.",
                                "Speeding’s risky, hon, ease off.",
                                "I’d rather arrive safe, love.",
                                "Not so fast, sweetie, please.",
                                "Careful—don’t need a crash."
                            }}
                        }},
                    {"sudden_stop", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Whoa, man, my laptop!",
                                "Rough stop, dude, careful!",
                                "Phone’s on the floor, bro!",
                                "That wasn’t planned, pal!",
                                "I’m on a call—easy, man."
                            }},
                            { Gender.Female, new List<string> {
                                "Oh, love, my laptop almost fell!",
                                "That stop was harsh, hon!",
                                "My phone slid off, dear!",
                                "Careful, sweetie, that was rough!",
                                "On a call—please, no more."
                            }}
                        }},
                    {"smooth", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Smooth ride, man, thanks.",
                                "Good driving, dude, appreciate it.",
                                "Perfect for my notes, bro.",
                                "You’re chilling my stress, pal.",
                                "Just what I needed, man."
                            }},
                            { Gender.Female, new List<string> {
                                "Lovely smooth ride, dear.",
                                "Thanks for the careful drive, hon.",
                                "Great for reviewing notes, love.",
                                "You’re saving my day, sweetie.",
                                "Exactly the ride I wanted."
                            }}
                        }},
                    {"collision", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Unacceptable, man, what was that?",
                                "Gotta report this, dude.",
                                "My firm won’t like this, bro!",
                                "That crash killed my call, pal!",
                                "This suit’s worth more than that hit!"
                            }},
                            { Gender.Female, new List<string> {
                                "This is outrageous, dear!",
                                "I’ll have to report this, hon.",
                                "My company’s not happy, love!",
                                "That crash ruined my call, sweetie!",
                                "This outfit’s too nice for crashes!"
                            }}
                        }}
                }
            },
            { PassengerType.Tourist, new Dictionary<string, Dictionary<Gender, List<string>>>
                {
                    {"regular", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "First time here, man—any tips?",
                                "Best eats around, dude?",
                                "How long you driven here, bro?",
                                "These buildings are wild!",
                                "City’s nuts compared to home.",
                                "Got BBQ spots, man",
                                "Tacos here legit?",
                                "Here for the weekend, tryna see it all!",
                                "Hot as hell out here, dude",
                                "No skyscrapers like this back home!",
                                "Museums worth it",
                                "This place is dope!",
                                "Nightlife tips, dude?",
                                "Snapping pics for my blog!",
                                "Food carts look sick, bro!",
                                "GPS is trash—glad you got this!",
                                "Feels like a movie, man!",
                                "You ever bored driving?"
                            }},
                            { Gender.Female, new List<string> {
                                "First visit—any must-sees?",
                                "Where’s good food, mon amie?",
                                "You driven long here?",
                                "Love the architecture!",
                                "So different from home.",
                                "Any BBQ places?",
                                "Heard the tacos?",
                                "Just a weekend trip—wanna see it all!",
                                "Super warm out here",
                                "My town’s got nothing this tall!",
                                "Good museums, arigatou?",
                                "This city’s amazing",
                                "Know cool nightlife",
                                "Getting pics for my blog!",
                                "Those food carts—yum!",
                                "I’d be lost without you!",
                                "Like a film set!",
                                "Driving here fun, or tiring?"
                            }}
                        }},
                    {"dynamic", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "The {0} was unreal—first time seeing a place like that, man!",
                                "Checked out the {0}, now to the {1}—this city’s wild, dude!",
                                "Man, the {0} blew my mind, heading to the {1}—any tips, bro?",
                                "Never seen a {0} like that, off to the {1}—loving this trip, pal!"
                            }},
                            { Gender.Female, new List<string> {
                                "The {0} was amazing—nothing like that back home, hon!",
                                "Visited the {0}, now going to the {1}—this place is incredible, love!",
                                "Wow, the {0} was so cool, heading to the {1}—any suggestions, dear?",
                                "First time at a {0} like that, off to the {1}—best trip ever, sweetie!"
                            }}
                        }},
                    {"flirty", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Hey, handsome, show me the city?",
                                "You’re my favorite tour guide, cutie!",
                                "This trip’s better with you, stud!"
                            }},
                            { Gender.Female, new List<string> {
                                "Hi, gorgeous, know the best spots?",
                                "My fave tour guide, sweetie!",
                                "You’re making this trip amazing, dear!"
                            }}
                        }},
                    {"speeding", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Man, we’re zooming! Normal here?",
                                "Hope this ain’t illegal, dude!",
                                "Crazier than my town, bro!",
                                "Slow down, man, I wanna see stuff!",
                                "Not a race, pal, chill!"
                            }},
                            { Gender.Female, new List<string> {
                                "Wow, hon, we’re flying! Usual?",
                                "Not breaking laws, are we, love?",
                                "Wilder than home, dear!",
                                "Ease up, sweetie, I love the views!",
                                "My guide didn’t mention racing!"
                            }}
                        }},
                    {"sudden_stop", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Whoa, dude, what’s that about?!",
                                "My camera almost broke, man!",
                                "Souvenirs hit the floor, bro!",
                                "Not on the plan, pal!",
                                "My map’s trashed now, dude!"
                            }},
                            { Gender.Female, new List<string> {
                                "Oh, love, that was sudden!",
                                "Nearly lost my camera, hon!",
                                "My souvenirs fell, dear!",
                                "Wasn’t expecting that, sweetie!",
                                "My map’s all messed up, love!"
                            }}
                        }},
                    {"smooth", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Nice and smooth, man, love it!",
                                "Good ride, dude, thanks!",
                                "Perfect for sightseeing, bro!",
                                "Making my trip epic, pal!",
                                "Smooth for my pics, man!"
                            }},
                            { Gender.Female, new List<string> {
                                "So pleasant, love, thank you!",
                                "Great ride, hon, perfect!",
                                "Ideal for views, dear!",
                                "You’re making my trip, sweetie!",
                                "Smooth for my photos, love!"
                            }}
                        }},
                    {"collision", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Whoa, man, part of the tour?!",
                                "Not a crash course, dude!",
                                "My insurance better cover this, bro!",
                                "Not in the guidebook, pal!",
                                "Here for sights, not wrecks, man!"
                            }},
                            { Gender.Female, new List<string> {
                                "Oh, hon, is this the tour?!",
                                "Didn’t sign up for crashes, love!",
                                "Hope my insurance is good, dear!",
                                "Not on the itinerary, sweetie!",
                                "Came for culture, not chaos, hon!"
                            }}
                        }}
                }
            },
            { PassengerType.Party, new Dictionary<string, Dictionary<Gender, List<string>>>
                {
                    {"regular", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Let’s kick this off, bro!",
                                "Ready for a wild ride, man?",
                                "Yo, dude, got an aux cord?",
                                "Hitting clubs—any spots, pal?",
                                "This ride’s our pregame, bro!",
                                "Crank the music, man!",
                                "City’s crazy—you party, dude?",
                                "Late for VIP, let’s roll!",
                                "This car’s got vibes, bro!",
                                "Seen wild nights, right, pal?",
                                "Headed to the hottest club, man!",
                                "Join us after, dude?",
                                "Keep the energy high, bro!",
                                "Got snacks in here, man?",
                                "Hurry so we can dance, pal!",
                                "Where’s the afterparty, dude?",
                                "You’re our VIP driver, bro!",
                                "Hope you’re cool with loud, man!"
                            }},
                            { Gender.Female, new List<string> {
                                "Party time, let’s go!",
                                "Gonna be a fun ride?",
                                "Hey, where’s the aux?",
                                "Clubs tonight—any faves?",
                                "This ride’s our warm-up!",
                                "Blast some tunes!",
                                "This city’s lit—you party?",
                                "We’re late for VIP—floor it!",
                                "This car’s giving vibes!",
                                "Bet you’ve seen epic nights!",
                                "Off to the best club!",
                                "Come hang after?",
                                "Match our vibe—upbeat!",
                                "Any snacks here?",
                                "Get us there quick to dance!",
                                "Know the afterparty spot?",
                                "Our VIP driver, right?",
                                "Hope you’re down for loud!"
                            }}
                        }},
                    {"dynamic", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Just left the {0}, hitting the {1}—keep it lit, bro!",
                                "From the {0} to the {1}—crank the tunes, man!",
                                "Coming from the {0}, off to the {1}—VIP vibes, dude!",
                                "Party at the {0} was wild, now to the {1}—fast, pal!"
                            }},
                            { Gender.Female, new List<string> {
                                "Dancing at the {0}, now to the {1}—keep it fun, hon!",
                                "Left the {0}, heading to the {1}—music up, love!",
                                "From the {0} to the {1}—feeling epic, dear!",
                                "{0} was awesome, off to the {1}—hurry, sweetie!"
                            }}
                        }},
                    {"dynamic_same_type", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Back to the {0} for round two—turn it up, bro!",
                                "Nightclub again—let’s keep the party rolling, dude!",
                                "Same {0}, new energy—crank it, man!",
                                "Round two at the {0}—keep it lit, pal!"
                            }},
                            { Gender.Female, new List<string> {
                                "Back to the {0} for more—keep it fun, hon!",
                                "Another {0} night—let’s dance again, love!",
                                "Same {0}, more vibes—turn it up, dear!",
                                "Round two at the {0}—so exciting, sweetie!"
                            }}
                        }},
                    {"flirty", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Hey, hon, let’s make this fun!",
                                "Looking cool, driver—party with us?",
                                "You’re turning up my night, babe!"
                            }},
                            { Gender.Female, new List<string> {
                                "Hey, hon, let’s make this fun!",
                                "Looking cool, driver—party with us?",
                                "You’re turning up my night, babe!"
                            }}
                        }},
                    {"speeding", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Hell yeah, bro, floor it!",
                                "Rollercoaster vibes, man!",
                                "Keep it fast, dude, love it!",
                                "Go, pal, we’re hyped!",
                                "Speed’s our jam, bro!"
                            }},
                            { Gender.Female, new List<string> {
                                "Woo, girl, speed it up!",
                                "Like a thrill ride, hon!",
                                "Stay fast, love, it’s awesome!",
                                "Keep going, sweetie, we’re pumped!",
                                "This speed’s fire, dear!"
                            }}
                        }},
                    {"sudden_stop", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Yo, man, party foul!",
                                "Killed the vibe, dude!",
                                "My drink’s everywhere, bro!",
                                "Chill the brakes, pal!",
                                "Stop messed us up, man!"
                            }},
                            { Gender.Female, new List<string> {
                                "Oh, hon, total buzzkill!",
                                "That stop was lame, love!",
                                "Spilled my drink, dear!",
                                "Easy on brakes, sweetie!",
                                "Mood’s gone, hon, ugh!"
                            }}
                        }},
                    {"smooth", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Smooth moves, bro, nice!",
                                "Chill ride, man, love it.",
                                "Keeping us hyped, dude!",
                                "Good driving, pal, vibes!",
                                "Cruising like a boss, bro!"
                            }},
                            { Gender.Female, new List<string> {
                                "Sweet moves, hon, perfect!",
                                "Such a cool ride, love!",
                                "You’re keeping us pumped, dear!",
                                "Awesome driving, sweetie!",
                                "Cruising so well, hon!"
                            }}
                        }},
                    {"collision", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Bro, save it for bumper cars!",
                                "Crash ain’t it, man!",
                                "Party’s done if you keep that up!",
                                "No crashing, dude, dance time!",
                                "Not the vibe, pal!"
                            }},
                            { Gender.Female, new List<string> {
                                "Girl, not bumper cars!",
                                "Crash killed it, hon!",
                                "No wrecks, love, we’re partying!",
                                "Dance, not crash, sweetie!",
                                "Wrong mood, dear, ugh!"
                            }}
                        }}
                }
            },
            { PassengerType.Silent, new Dictionary<string, Dictionary<Gender, List<string>>>
                {
                    {"regular", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "...",
                                "*looks out window*",
                                "*checks phone*",
                                "*nods at the guy*",
                                "*shifts bag*",
                                "*quiet sigh*"
                            }},
                            { Gender.Female, new List<string> {
                                "...",
                                "*stares out window*",
                                "*glances at phone*",
                                "*nods at the lady*",
                                "*adjusts bag*",
                                "*soft sigh*"
                            }}
                        }},
                    {"dynamic", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "*nods from the {0}*",
                                "*glances toward the {1}*",
                                "*sighs, leaving the {0}*",
                                "*quiet look at the {1}*"
                            }},
                            { Gender.Female, new List<string> {
                                "*nods from the {0}*",
                                "*eyes the {1}*",
                                "*sighs, from the {0}*",
                                "*calm glance at the {1}*"
                            }}
                        }},
                    {"flirty", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {}},
                            { Gender.Female, new List<string> {}}
                        }},
                    {"speeding", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "*frowns at the guy*",
                                "*grips seat*",
                                "*sigh*",
                                "*eyes the driver*",
                                "*leans forward*"
                            }},
                            { Gender.Female, new List<string> {
                                "*frowns at the lady*",
                                "*holds seat*",
                                "*sigh*",
                                "*glances over*",
                                "*shifts nervously*"
                            }}
                        }},
                    {"sudden_stop", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "Watch it, man!",
                                "*glares at the guy*",
                                "*mutters*",
                                "*jaw tightens*",
                                "*sharp breath*"
                            }},
                            { Gender.Female, new List<string> {
                                "Careful, lady!",
                                "*glares at the driver*",
                                "*mumbles*",
                                "*clenches fists*",
                                "*quick exhale*"
                            }}
                        }},
                    {"smooth", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "*nods at the guy*",
                                "relaxes a bit*",
                                "*slight smile*",
                                "*sits easy*",
                                "*calm look*"
                            }},
                            { Gender.Female, new List<string> {
                                "*nods at the lady*",
                                "*eases back*",
                                "*tiny smile*",
                                "*rests calmly*",
                                "*content glance*"
                            }}
                        }},
                    {"collision", new Dictionary<Gender, List<string>>
                        {
                            { Gender.Male, new List<string> {
                                "*glares hard at the guy*",
                                "Don’t do that again, man.",
                                "*mutters angrily*",
                                "*shakes head*",
                                "*stares coldly*"
                            }},
                            { Gender.Female, new List<string> {
                                "Not again, please.",
                                "*glares at the lady*",
                                "*grumbles*",
                                "*shakes head*",
                                "*icy stare*"
                            }}
                        }}
                }
            }
        };

        public static string GetRandomComment(PassengerType passengerType, string commentType, Gender playerGender)
        {
            if (!Comments.ContainsKey(passengerType) ||
                !Comments[passengerType].ContainsKey(commentType) ||
                !Comments[passengerType][commentType].ContainsKey(playerGender))
            {
                return string.Empty;
            }
            var comments = Comments[passengerType][commentType][playerGender];
            if (comments.Count == 0) return string.Empty;
            string key = $"passenger_{passengerType}_{commentType}_{playerGender}";
            return comments[ConversationTracker.GetRandomLineIndex(comments, key)];
        }
    }
}