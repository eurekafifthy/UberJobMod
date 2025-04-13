using System.Collections.Generic;

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
                        "Traffic’s wild around here, huh, guy?",
                        "You ever had any big shots in your car?",
                        "This city’s got some crazy energy, man!",
                        "Just running errands—boring stuff, dude.",
                        "Know a good coffee spot, my man?",
                        "My last driver got lost—you seem solid!",
                        "Traffic’s a mess, but you’re handling it.",
                        "I’m late for dinner—wife’s gonna kill me!",
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
                                "Warn me next time, guy!"
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
                                "Bumper cars, huh, guy?"
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
                        "First time here, man—any tips, amigo?",
                        "Best eats around, dude, mon ami?",
                        "How long you driven here, bro?",
                        "These buildings are wild, vraiment!",
                        "City’s nuts compared to home, यार.",
                        "Got BBQ spots, man, compañero?",
                        "Tacos here legit, bro—verdad?",
                        "Here for the weekend, tryna see it all!",
                        "Hot as hell out here, dude, ね？",
                        "No skyscrapers like this back home!",
                        "Museums worth it, man, arigatou?",
                        "This place is dope, bro, danke!",
                        "Nightlife tips, dude, por favor?",
                        "Snapping pics for my blog, man!",
                        "Food carts look sick, bro, amico!",
                        "GPS is trash—glad you got this!",
                        "Feels like a movie, man, incroyable!",
                        "You ever bored driving, dude?"
                    }},
                    { Gender.Female, new List<string> {
                        "First visit—any must-sees, amiga?",
                        "Where’s good food, mon amie?",
                        "You driven long here, señora?",
                        "Love the architecture, vraiment!",
                        "So different from home, यार.",
                        "Any BBQ places, compañera?",
                        "Heard the tacos rock—verdad?",
                        "Just a weekend trip—wanna see it all!",
                        "Super warm out here, ね？",
                        "My town’s got nothing this tall!",
                        "Good museums, arigatou?",
                        "This city’s amazing, danke!",
                        "Know cool nightlife, por favor?",
                        "Getting pics for my blog!",
                        "Those food carts—yum, amica!",
                        "I’d be lost without you!",
                        "Like a film set, incroyable!",
                        "Driving here fun, or tiring?"
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
                                "*relaxes a bit*",
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
                                "*glares at the lady*",
                                "Not again, please.",
                                "*grumbles*",
                                "*shakes head*",
                                "*icy stare*"
                            }}
                        }}
                }
            }
        };
    }
}