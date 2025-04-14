using MelonLoader;
using MelonLoader.Utils;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Il2Cpp;
using Il2CppUI.PlayerHUD;
using Il2CppBuildings.Outdoors;
using Il2CppCharacter;
using Il2CppBigAmbitions.Characters;
using Il2CppUI.Smartphone.Apps.Persona;
using Il2CppVehicles.VehicleTypes;
using Il2CppEnums;
using Newtonsoft.Json;
using UberSideJobMod.UberSideJobMod;

namespace UberSideJobMod
{
    public class UberJobMod : MelonMod
    {
        #region Fields

        // Constants
        private const float CheckDistance = 75f;
        private const float PickupConfirmTime = 0.05f;
        private float timeSinceLastOutsideZone = 0f;
        private const float PassengerOfferInterval = 30f;
        private const float UberJobCooldown = 120f;
        private const float MaxWaitingTime = 300f;
        private const float ConversationInterval = 20f;
        private const float CollisionRecoveryTime = 8f;
        private const float AccelerationThreshold = 10f;
        private const float SmoothAccelerationThreshold = 15f;
        private const float PeakMultiplier = 1.5f;
        private const float SuddenStopCooldownDuration = 10f;
        private const float SpeedingNotificationCooldown = 25f;
        private bool isCompletingJob = false;

        // Tips System
        private readonly float baselineTipRate = 0.15f;
        private readonly float tipMultiplierForGoodDriving = 1.5f;
        private readonly float tipMultiplierForBadDriving = 0.5f;
        private readonly Dictionary<PassengerType, float> passengerTypeTipRates = new Dictionary<PassengerType, float>();
        private readonly Dictionary<string, float> neighborhoodTipRates = new Dictionary<string, float>();

        // Notification Messages
        private readonly Dictionary<PassengerType, Dictionary<string, List<string>>> notificationMessages = new Dictionary<PassengerType, Dictionary<string, List<string>>>
        {
            [PassengerType.Business] = new Dictionary<string, List<string>>
            {
                ["speeding"] = new List<string>
                {
                    "{playerName}, I’m on a tight schedule—can you slow down?",
                    "{playerName}, this speed is unprofessional, please ease up!",
                    "{playerName}, slow down, I’d like to arrive in one piece!"
                },
                ["sudden_stop"] = new List<string>
                {
                    "{playerName}, that stop was abrupt—fortunately, I’m belted in!",
                    "{playerName}, please, no more sudden stops, I have a meeting!",
                    "{playerName}, a smoother ride would be much appreciated!"
                }
            },
            [PassengerType.Tourist] = new Dictionary<string, List<string>>
            {
                ["speeding"] = new List<string>
                {
                    "{playerName}, whoa, slow down—I want to enjoy the sights!",
                    "{playerName}, can you ease up? I’m here to explore, not race!",
                    "{playerName}, slow down, I’d love to take some photos!"
                },
                ["sudden_stop"] = new List<string>
                {
                    "{playerName}, that stop startled me—good thing I’m buckled up!",
                    "{playerName}, oh my, let’s avoid those sudden stops, okay?",
                    "{playerName}, that was a jolt—let’s keep it smooth, please!"
                }
            },
            [PassengerType.Party] = new Dictionary<string, List<string>>
            {
                ["speeding"] = new List<string>
                {
                    "{playerName}, yo, slow down—we’re not at the club yet!",
                    "{playerName}, chill on the speed, let’s keep the vibes cool!",
                    "{playerName}, ease up, I don’t wanna spill my drink!"
                },
                ["sudden_stop"] = new List<string>
                {
                    "{playerName}, whoa, that stop—luckily I’m strapped in!",
                    "{playerName}, easy on the brakes, let’s keep the party going!",
                    "{playerName}, that stop killed the vibe—smoother, please!"
                }
            },
            [PassengerType.Silent] = new Dictionary<string, List<string>>
            {
                ["speeding"] = new List<string>
                {
                    "{playerName}, *sighs* Can you slow down?",
                    "{playerName}, *frowns* Please, less speed.",
                    "{playerName}, *quietly* Slow down, okay?"
                },
                ["sudden_stop"] = new List<string>
                {
                    "{playerName}, *grips seat* I’m belted, but still…",
                    "{playerName}, *mutters* That stop was harsh.",
                    "{playerName}, *silent glare* Smoother, please."
                }
            },
            [PassengerType.Regular] = new Dictionary<string, List<string>>
            {
                ["speeding"] = new List<string>
                {
                    "{playerName}, can you slow down a bit, please?",
                    "{playerName}, let’s take it easy on the speed, okay?",
                    "{playerName}, slow down, I’m not in a rush!"
                },
                ["sudden_stop"] = new List<string>
                {
                    "{playerName}, that stop was rough—good thing I’m buckled!",
                    "{playerName}, let’s keep it smooth, that stop was jarring!",
                    "{playerName}, whoa, easy on the brakes, please!"
                }
            }
        };

        // Core State
        private bool uberJobActive = false;
        private bool showUberUI = false;
        private UberPassenger currentPassenger = null;
        private DriverStats driverStats = new DriverStats();
        private List<Address> addresses = new List<Address>();
        private UberUIManager uiManager;
        private UberNotificationUI notificationUI;

        // Game Objects and References
        private GameObject pickupCircle;
        private GameObject dropoffCircle;
        private PlayerHUD playerHUD = null;
        private Il2Cpp.Address lastClosestBuildingAddress = null;
        private string currentNeighborhood = "Unknown";
        private readonly Dictionary<Il2Cpp.Address, Vector3> buildingPositions = new Dictionary<Il2Cpp.Address, Vector3>();

        // Vehicle
        private VehicleController lastDetectedVehicle = null;
        private string lastVehicleTypeName = null;
        private VehicleCategory currentVehicleCategory = VehicleCategory.Standard;
        private float? maxVehicleSpeed = null;
        private float? vehiclePrice = null;
        private float? enginePower = null;
        private bool autoParkSupported = false;
        private bool isCheapCar = false;

        // Driving State Tracking
        private float currentSpeed = 0f;
        private float previousSpeed = 0f;
        private float previousTime = 0f;
        private float smoothDrivingTime = 0f;
        private bool isSpeeding = false;
        private bool isCollision = false;
        private bool hasSpeedLimiterModule = false;
        private float gameSpeedLimit = float.MaxValue;
        private float dynamicSpeedLimit = 50f;
        private float speedingTolerance = 5f;
        private float suddenStopCooldown = 0f;
        private float lastCollisionTime = 0f;
        private float lastSuddenStopTime = -100f;
        private float lastSpeedingNotificationTime = -100f;
        private float timeAtPickupAddress = 0f;

        // Timing and Intervals
        private float lastPassengerOfferTime = 0f;
        private float lastJobCompletionTime = -UberJobCooldown;
        private float lastConversationTime = 0f;

        // UI and Conversation State
        private bool conversationShown = false;
        private bool ratingPending = false;
        private bool hadRegularConversation = false;
        private int conversationCount = 0;
        private bool isPeakHours = false;

        // Player State
        private Gender playerGender;
        private CharacterData playerCharacterData;
        private string playerName;

        #endregion

        #region Initialization

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Uber Mod Big Ambitions - loaded successfully!");
            InitializeComponents();
            LoadDriverStats();
            InitializeTipSystem();
            MelonCoroutines.Start(WaitForGameLoad());
        }

        private void InitializeComponents()
        {
            uiManager = new UberUIManager();
            uiManager.InitializeUI();
            uiManager.OnAcceptClicked += OnAcceptButtonClicked;
            uiManager.OnDeclineClicked += OnDeclineButtonClicked;
            uiManager.OnCancelClicked += OnCancelButtonClicked;
            uiManager.SetUIVisible(false);

            notificationUI = new UberNotificationUI();
            notificationUI.Initialize();
        }

        private void InitializeTipSystem()
        {
            passengerTypeTipRates[PassengerType.Business] = 0.25f;
            passengerTypeTipRates[PassengerType.Tourist] = 0.20f;
            passengerTypeTipRates[PassengerType.Party] = 0.15f;
            passengerTypeTipRates[PassengerType.Silent] = 0.10f;
            passengerTypeTipRates[PassengerType.Regular] = 0.15f;

            neighborhoodTipRates["Midtown"] = 0.25f;
            neighborhoodTipRates["LowerManhattan"] = 0.15f;
            neighborhoodTipRates["HellsKitchen"] = 0.15f;
            neighborhoodTipRates["GarmentDistrict"] = 0.10f;
            neighborhoodTipRates["MurrayHill"] = 0.20f;

            if (driverStats.tipsByPassengerType == null)
            {
                driverStats.tipsByPassengerType = new Dictionary<PassengerType, float>();
            }

            foreach (PassengerType type in System.Enum.GetValues(typeof(PassengerType)))
            {
                if (!driverStats.tipsByPassengerType.ContainsKey(type))
                {
                    driverStats.tipsByPassengerType[type] = 0f;
                }
            }
        }

        private IEnumerator WaitForGameLoad()
        {
            while (CityManager.Instance == null || CityManager.Instance.CityBuildingControllersDictionary == null)
            {
                yield return new WaitForSeconds(0.5f);
            }

            while (playerHUD == null)
            {
                playerHUD = GameObject.FindObjectOfType<PlayerHUD>();
                yield return new WaitForSeconds(0.5f);
            }

            yield return LoadPlayerCharacterData();

            LoadAddressData();

            if (addresses.Count == 0)
            {
                yield return new WaitForSeconds(10f);
                LoadAddressData();
            }

            MelonCoroutines.Start(EfficientUberJobRoutine());
            MelonCoroutines.Start(TimeCheckRoutine());
        }

        private IEnumerator LoadPlayerCharacterData()
        {
            const float timeout = 15f;
            float elapsed = 0f;

            while (playerCharacterData == null && elapsed < timeout)
            {
                try
                {
                    playerCharacterData = Il2CppHelpers.PlayerHelper.GetPlayerCharacterData();
                }
                catch
                {
                    // Retry silently
                }

                if (playerCharacterData == null)
                {
                    yield return new WaitForSeconds(0.5f);
                    elapsed += 0.5f;
                }
            }

            try
            {
                if (playerCharacterData != null)
                {
                    var rawGender = playerCharacterData.gender;

                    playerGender = rawGender switch
                    {
                        Il2CppBigAmbitions.Characters.Gender.Male => Gender.Male,
                        Il2CppBigAmbitions.Characters.Gender.Female => Gender.Female,
                        _ => Gender.Male
                    };

                    if (rawGender != Il2CppBigAmbitions.Characters.Gender.Male &&
                        rawGender != Il2CppBigAmbitions.Characters.Gender.Female)
                    {
                    }

                    playerName = playerCharacterData.name ?? "Driver";
                }
                else
                {
                    playerGender = Gender.Male;
                    playerName = "Driver";
                }
            }
            catch (Exception ex)
            {
                playerGender = Gender.Male;
                playerName = "Driver";
            }

            if (string.IsNullOrEmpty(playerName))
            {
                yield return LoadPlayerNameFromCharacterInfo();
            }
        }

        private IEnumerator LoadPlayerNameFromCharacterInfo()
        {
            const float timeout = 15f;
            float elapsed = 0f;
            CharacterInfo characterInfo = null;

            while (characterInfo == null && elapsed < timeout)
            {
                characterInfo = GameObject.FindObjectOfType<CharacterInfo>();
                if (characterInfo == null)
                {
                    yield return new WaitForSeconds(0.5f);
                    elapsed += 0.5f;
                }
            }

            if (characterInfo != null && characterInfo.characterName != null)
            {
                playerName = characterInfo.characterName.text;
            }

            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Driver";
            }
        }

        private void LoadAddressData()
        {
            try
            {
                addresses.Clear();
                buildingPositions.Clear();
                var cityManager = CityManager.Instance;

                foreach (var pair in cityManager.CityBuildingControllersDictionary)
                {
                    var gameAddress = pair.Key;
                    var buildingController = pair.Value;
                    var buildingPos = buildingController.transform.position;

                    if (buildingPositions.ContainsKey(gameAddress))
                    {
                        continue;
                    }

                    buildingPositions[gameAddress] = buildingPos;

                    var buildingRegistration = buildingController.buildingRegistration;
                    string businessName = "Unknown Business";
                    string buildingType = "Unknown";
                    BusinessTypeName businessType = BusinessTypeName.Empty;
                    string neighborhood = "Unknown Area";

                    if (buildingRegistration != null)
                    {
                        try
                        {
                            businessName = buildingRegistration.BusinessName ?? "Unknown Business";
                            var buildingTypeObj = buildingRegistration.GetBuildingType();
                            buildingType = buildingTypeObj != null ? buildingTypeObj.ToString() : "Unknown";
                            businessType = buildingRegistration.businessTypeName;
                            var neighborhoodObj = buildingRegistration.GetNeighborhood();
                            neighborhood = neighborhoodObj != null ? neighborhoodObj.ToString() : "Unknown Area";

                            if (buildingType.ToLower() is "residential" or "warehouse" or "special")
                            {
                                businessType = BusinessTypeName.Empty;
                                businessName = "Unknown Business";
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }

                    if (addresses.Any(a => a.gameAddress == gameAddress))
                    {
                        continue;
                    }

                    addresses.Add(new Address
                    {
                        gameAddress = gameAddress,
                        address = businessName,
                        neighborhood = neighborhood,
                        businessName = businessName,
                        businessType = businessType,
                        buildingType = buildingType,
                        totalSize = 0,
                        customerCapacity = 0,
                        trafficIndex = 0,
                        parkingZone = "Unknown"
                    });
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void LoadDriverStats()
        {
            try
            {
                string saveDir = Path.Combine(MelonEnvironment.UserDataDirectory, "UberJobMod");
                string savePath = Path.Combine(saveDir, "DriverStats.json");

                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }

                if (File.Exists(savePath))
                {
                    string json = File.ReadAllText(savePath);
                    driverStats = JsonConvert.DeserializeObject<DriverStats>(json);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error loading driver stats: {ex.Message}");
                driverStats = new DriverStats();
            }
        }

        #endregion

        #region Coroutines

        private IEnumerator EfficientUberJobRoutine()
        {
            float idleTimeWithoutJob = 0f;
            float lastDrivingStateUpdate = 0f;

            while (true)
            {
                float waitTime = uberJobActive ? 0.5f : 1f;

                if (showUberUI || uberJobActive)
                {
                    UpdateNeighborhood();
                    VehicleController playerVehicle = GetPlayerVehicle();

                    if (isCompletingJob)
                    {
                        yield return new WaitForSeconds(waitTime);
                        continue;
                    }

                    if (playerVehicle != null && showUberUI && !uberJobActive && currentPassenger == null)
                    {
                        idleTimeWithoutJob += waitTime;
                        if (Time.time - lastJobCompletionTime > UberJobCooldown &&
                            (Time.time - lastPassengerOfferTime > PassengerOfferInterval || idleTimeWithoutJob > 45f))
                        {
                            OfferNewPassenger();
                            lastPassengerOfferTime = Time.time;
                            idleTimeWithoutJob = 0f;
                        }
                    }
                    else
                    {
                        idleTimeWithoutJob = 0f;
                    }

                    if (uberJobActive && currentPassenger != null)
                    {
                        if (currentPassenger != null && currentPassenger.isPickedUp && !conversationShown && Time.time - lastConversationTime > ConversationInterval)
                        {
                            ShowPassengerConversation();
                            lastConversationTime = Time.time;
                        }

                        if (currentPassenger != null && !currentPassenger.isPickedUp)
                        {
                            currentPassenger.waitTime += waitTime;
                            if (currentPassenger.waitTime > MaxWaitingTime && UnityEngine.Random.Range(0, 100) < 5)
                            {
                                PassengerCancelRide("The passenger got tired of waiting and cancelled the ride.");
                            }
                        }

                        if (playerVehicle != null)
                        {
                            CheckDestinationReached(playerVehicle);
                        }
                        else
                        {
                        }

                        if (currentPassenger != null && currentPassenger.isPickedUp && Time.time - lastDrivingStateUpdate >= 0.2f)
                        {
                            if (playerVehicle != null)
                            {
                                UpdateDrivingState(playerVehicle);
                                lastDrivingStateUpdate = Time.time;
                            }
                            else
                            {
                            }
                        }
                    }
                }

                yield return new WaitForSeconds(waitTime);
            }
        }

        private IEnumerator TimeCheckRoutine()
        {
            while (true)
            {
                try
                {
                    int currentHour = Il2Cpp.TimeHelper.CurrentHour;
                    isPeakHours = (currentHour >= 7 && currentHour <= 9) || (currentHour >= 16 && currentHour <= 19);

                    if (isPeakHours)
                    {
                    }
                }
                catch (Exception ex)
                {
                }

                yield return new WaitForSeconds(60f);
            }
        }

        private IEnumerator ResetConversationFlag()
        {
            yield return new WaitForSeconds(10f);
            conversationShown = false;
        }

        private IEnumerator RequestRatingAfterDelay(float delay, UberPassenger passenger)
        {
            yield return new WaitForSeconds(delay);

            float baseRating = UnityEngine.Random.Range(3.5f, 5.1f);
            float ratingDeduction = 0f;
            List<string> feedbackReasons = new List<string>();

            if (passenger.waitTime > 120f)
            {
                float waitDeduction = UnityEngine.Random.Range(0.3f, 0.7f);
                ratingDeduction += waitDeduction;
                feedbackReasons.Add("a late pickup");
            }

            if (passenger.suddenStopCount > 0)
            {
                float stopDeduction = passenger.suddenStopCount * 0.1f;
                ratingDeduction += stopDeduction;
                feedbackReasons.Add("sudden stops");
            }

            if (passenger.totalSpeedingTime > 30f && (!isCheapCar || maxVehicleSpeed > dynamicSpeedLimit * 1.3f))
            {
                float speedDeduction = Mathf.Min(0.3f, passenger.totalSpeedingTime / 100f) * (isCheapCar ? 0.5f : 1f);
                ratingDeduction += speedDeduction;
                feedbackReasons.Add("speeding");
            }

            if (passenger.collisionCount > 0)
            {
                float collisionDeduction = passenger.collisionCount * 0.5f;
                ratingDeduction += collisionDeduction;
                feedbackReasons.Add(passenger.collisionCount == 1 ? "a crash" : "multiple crashes");
            }

            ratingDeduction = Mathf.Min(ratingDeduction, 3.5f);
            float finalRating = Mathf.Clamp(baseRating - ratingDeduction, 1f, 5f);

            if (passenger.collisionCount > 0)
            {
                finalRating = Mathf.Min(finalRating, 4.0f);
            }

            if (passenger.collisionCount >= 3)
            {
                finalRating = Mathf.Min(finalRating, 2.5f);
            }

            string feedbackMessage = feedbackReasons.Count == 1 ? $"Because of {feedbackReasons[0]}." :
                                    feedbackReasons.Count > 1 ? $"Because of {string.Join(", ", feedbackReasons.Take(feedbackReasons.Count - 1))} and {feedbackReasons.Last()}." : "";

            driverStats.ratings.Add(finalRating);
            driverStats.averageRating = driverStats.ratings.Average();

            string notificationMessage = $"{passenger.passengerName} rated you {finalRating:F1} stars!" +
                                        (string.IsNullOrEmpty(feedbackMessage) ? "" : $"\n{feedbackMessage}");
            NotificationType notificationType = finalRating >= 4f ? NotificationType.Success :
                                               finalRating >= 3f ? NotificationType.Info :
                                               NotificationType.Warning;

            ShowNotification(notificationMessage, notificationType);
            ratingPending = false;
            SaveDriverStats();
            lastPassengerOfferTime = Time.time - PassengerOfferInterval + 15f;
        }

        private IEnumerator HideUIAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            showUberUI = false;
        }

        private IEnumerator ShowSpecialTouristConversation()
        {
            yield return new WaitForSeconds(8f);

            if (currentPassenger != null && currentPassenger.isPickedUp && currentPassenger.passengerType == PassengerType.Tourist)
            {
                string touristQuestion = $"Hey, what's the best place to visit in {currentNeighborhood}? I'm new here!";
                ShowNotification($"{currentPassenger.passengerName}: \"{touristQuestion}\"", NotificationType.Info, 15f);
                currentPassenger.hadSpecialConversation = true;
                currentPassenger.positiveInteractions++;
            }
        }

        #endregion

        #region Event Handlers

        public override void OnUpdate()
        {
            if (uiManager == null)
            {
                MelonLogger.Warning("uiManager is null in OnUpdate, initializing...");
                InitializeComponents();
            }

            if (uberJobActive && currentPassenger != null)
            {
                UpdateNeighborhood();
            }

            if (showUberUI)
            {
                UpdateUITitle();
                uiManager.UpdateUI(driverStats, currentPassenger, isPeakHours, uberJobActive);
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                ToggleUI();
            }
        }

        private void UpdateUITitle()
        {
            VehicleController playerVehicle = GetPlayerVehicle();

            if (playerVehicle != null && uiManager.titleText != null)
            {
                string vehicleName = playerVehicle.GetName()?.ToString();

                if (!string.IsNullOrEmpty(vehicleName))
                {
                    string newTitle = $"Uber | {playerName} | {vehicleName}";

                    if (uiManager.titleText.text != newTitle)
                    {
                        uiManager.titleText.text = newTitle;
                    }
                }
            }
        }

        private void ToggleUI()
        {
            showUberUI = !showUberUI;
            uiManager.SetUIVisible(showUberUI);

            if (showUberUI)
            {
                if (!uberJobActive && currentPassenger == null &&
                    Time.time - lastPassengerOfferTime > PassengerOfferInterval &&
                    Time.time - lastJobCompletionTime > UberJobCooldown)
                {
                    OfferNewPassenger();
                    lastPassengerOfferTime = Time.time - PassengerOfferInterval + 15f;
                }
            }
            else if (!uberJobActive && currentPassenger != null)
            {
                currentPassenger = null;
            }
        }

        private void OnAcceptButtonClicked()
        {
            uberJobActive = true;
            ShowNotification($"Ride accepted! Drive to pick up {currentPassenger.passengerName}\nat {currentPassenger.pickupAddress.DisplayName}.");
            CityManager.Instance.FindBuildingAndSetGuider(currentPassenger.pickupAddress.gameAddress, true);
            SpawnPickupCircle();
            lastCollisionTime = 0f;
            lastSuddenStopTime = -100f;
            suddenStopCooldown = 0f;
            isCollision = false;
        }

        private void OnDeclineButtonClicked()
        {
            uberJobActive = false;
            currentPassenger = null;
            showUberUI = true;
            lastJobCompletionTime = Time.time - UberJobCooldown + 60f;
            ShowNotification("Uber ride declined.");
            uiManager.UpdateUI(driverStats, currentPassenger, isPeakHours, uberJobActive);
        }

        private void OnCancelButtonClicked()
        {
            driverStats.cancelledRides++;
            uberJobActive = false;
            CleanupCircles();
            currentPassenger = null;
            showUberUI = true;
            ShowNotification("Uber ride canceled by driver.");
            SaveDriverStats();
            uiManager.UpdateUI(driverStats, currentPassenger, isPeakHours, uberJobActive);
            lastPassengerOfferTime = Time.time - PassengerOfferInterval + 15f;
        }

        public override void OnDeinitializeMelon()
        {
            uiManager?.DestroyUI();
            CleanupCircles();
        }

        #endregion

        #region Passenger Management

        private void OfferNewPassenger()
        {
            if (addresses.Count < 2)
            {
                return;
            }

            var pickup = addresses[UnityEngine.Random.Range(0, addresses.Count)];
            Address dropoff;

            do
            {
                dropoff = addresses[UnityEngine.Random.Range(0, addresses.Count)];
            } while (dropoff.gameAddress == pickup.gameAddress);

            if (!buildingPositions.ContainsKey(pickup.gameAddress) || !buildingPositions.ContainsKey(dropoff.gameAddress))
            {
                MelonLogger.Error("Could not find building positions for pickup or dropoff");
                return;
            }

            Vector3 pickupPos = buildingPositions[pickup.gameAddress];
            Vector3 dropoffPos = buildingPositions[dropoff.gameAddress];
            float distance = Vector3.Distance(pickupPos, dropoffPos);
            float fareMultiplier = isPeakHours ? PeakMultiplier : 1.0f;
            float fare = CalculateFare(distance) * fareMultiplier;

            PassengerType passengerType = DeterminePassengerType(pickup);
            Gender passengerGender = UnityEngine.Random.Range(0, 2) == 0 ? Gender.Male : Gender.Female;
            string passengerName = GeneratePassengerName(passengerGender);

            currentPassenger = new UberPassenger
            {
                pickupAddress = pickup,
                dropoffAddress = dropoff,
                pickupLocation = pickupPos,
                dropoffLocation = dropoffPos,
                fare = fare,
                isPickedUp = false,
                pickupTime = -1f,
                distanceToDestination = distance,
                passengerType = passengerType,
                passengerName = passengerName,
                conversationLines = PassengerDialogues.Comments[passengerType]["regular"]
                    .SelectMany(kvp => kvp.Value)
                    .ToArray(),
                collisionCount = 0,
                suddenStopCount = 0,
                totalSpeedingTime = 0f,
                gender = passengerGender
            };

            string surgeMessage = isPeakHours ? " (Surge Pricing Active!)" : "";
            string message = $"New Uber Passenger Request!{surgeMessage}\n" +
                             $"Passenger: {passengerName}\n" +
                             $"Pickup: {pickup.DisplayName}, {pickup.neighborhood}\n" +
                             $"Dropoff: {dropoff.DisplayName}, {dropoff.neighborhood}\n" +
                             $"Estimated Fare: ${fare:F2}";
            ShowNotification(message);
            showUberUI = true;
        }

        private PassengerType DeterminePassengerType(Address pickup)
        {
            PassengerType passengerType = (PassengerType)UnityEngine.Random.Range(0, 5);

            if (pickup.businessType == BusinessTypeName.JewelryStore || pickup.businessType == BusinessTypeName.Bank || pickup.businessType == BusinessTypeName.LawFirm)
            {
                passengerType = UnityEngine.Random.Range(0, 100) < 50 ? PassengerType.Business : passengerType;
            }
            else if (pickup.businessType == BusinessTypeName.Nightclub || pickup.businessType == BusinessTypeName.Casino)
            {
                passengerType = UnityEngine.Random.Range(0, 100) < 50 ? PassengerType.Party : passengerType;
            }
            else if (pickup.businessType == BusinessTypeName.GiftShop || pickup.businessType == BusinessTypeName.Florist)
            {
                passengerType = UnityEngine.Random.Range(0, 100) < 50 ? PassengerType.Tourist : passengerType;
            }
            else if (pickup.businessType == BusinessTypeName.Bookstore || pickup.businessType == BusinessTypeName.CoffeeShop)
            {
                passengerType = UnityEngine.Random.Range(0, 100) < 50 ? PassengerType.Regular : passengerType;
            }

            return passengerType;
        }

        private string GeneratePassengerName(Gender gender)
        {
            string firstName = gender == Gender.Male ?
                PassengerDialogues.MaleFirstNames[UnityEngine.Random.Range(0, PassengerDialogues.MaleFirstNames.Length)] :
                PassengerDialogues.FemaleFirstNames[UnityEngine.Random.Range(0, PassengerDialogues.FemaleFirstNames.Length)];
            string lastName = PassengerDialogues.LastNames[UnityEngine.Random.Range(0, PassengerDialogues.LastNames.Length)];
            return $"{firstName} {lastName}";
        }

        private void ShowPassengerConversation()
        {
            if (currentPassenger == null || !currentPassenger.isPickedUp)
            {
                return;
            }

            // Skip conversation for Silent passengers most of the time, unless there's a recent sudden stop
            if (currentPassenger.passengerType == PassengerType.Silent &&
                UnityEngine.Random.Range(0, 100) < 90 &&
                Time.time - lastSuddenStopTime >= 10f)
            {
                return;
            }

            string commentType;

            // First conversation or no regular conversation yet defaults to regular
            if (conversationCount == 0 || !hadRegularConversation)
            {
                commentType = "regular";
                hadRegularConversation = true;
            }
            else
            {
                // Determine comment type based on driving state, leveraging metrics updated in EfficientUberJobRoutine
                if (isCollision && currentPassenger.collisionCount > 0 && Time.time - lastCollisionTime < 10f && Time.time >= currentPassenger.pickupTime)
                {
                    commentType = "collision";
                }
                else if (Time.time - lastSuddenStopTime < 8f)
                {
                    commentType = "sudden_stop";
                }
                else if (isSpeeding && currentPassenger.totalSpeedingTime > 15f && currentSpeed <= (maxVehicleSpeed ?? 100f) * 0.9f)
                {
                    commentType = "speeding";
                }
                else if (smoothDrivingTime > 60f)
                {
                    commentType = "smooth";
                    smoothDrivingTime = 0f; // Reset to allow future smooth comments
                }
                else if (currentSpeed < 5f && Time.time - lastSuddenStopTime > 10f)
                {
                    var brakes = GetPlayerVehicle()?.gameObject?.GetComponent<Il2CppNWH.VehiclePhysics2.Brakes>();
                    if (brakes != null && brakes._isBraking && autoParkSupported && UnityEngine.Random.Range(0, 100) < 30)
                    {
                        commentType = "parking";
                    }
                    else
                    {
                        commentType = "regular";
                    }
                }
                else
                {
                    commentType = "regular";
                }
            }

            float effectiveEnginePower = enginePower ?? 45f;
            VehicleController playerVehicle = GetPlayerVehicle();
            string vehicleName = playerVehicle?.GetName()?.ToString();
            bool isHighEndVehicle = currentVehicleCategory == VehicleCategory.Luxury || currentVehicleCategory == VehicleCategory.Performance;

            string greeting = GetPassengerGreeting(commentType);
            bool canFlirt = ShouldUseFlirtyDialogue();

            if (commentType == "regular" && canFlirt && UnityEngine.Random.Range(0, 100) < 15)
            {
                commentType = "flirty";
            }

            if (commentType == "regular" && UnityEngine.Random.Range(0, 100) < 25)
            {
                ShowVehicleComment(greeting, vehicleName, isHighEndVehicle, effectiveEnginePower);
                return;
            }

            if (commentType == "parking")
            {
                ShowParkingComment(greeting, vehicleName, isHighEndVehicle);
                return;
            }

            commentType = AdjustCommentType(commentType);
            var comments = PassengerDialogues.Comments[currentPassenger.passengerType][commentType][playerGender];
            string conversation = comments[UnityEngine.Random.Range(0, comments.Count)];

            if (commentType == "regular" && (currentPassenger.passengerType == PassengerType.Business || currentPassenger.passengerType == PassengerType.Regular))
            {
                conversation = $"{greeting}{conversation}";
            }

            ShowNotification($"{currentPassenger.passengerName}: \"{conversation}\"", NotificationType.Info, 15f);
            conversationShown = true;
            conversationCount++;

            UpdatePassengerInteractions(commentType);

            if (currentPassenger.passengerType == PassengerType.Tourist &&
                commentType == "regular" &&
                conversationCount >= 2 &&
                !currentPassenger.hadSpecialConversation &&
                UnityEngine.Random.Range(0, 100) < 40)
            {
                MelonCoroutines.Start(ShowSpecialTouristConversation());
            }

            MelonCoroutines.Start(ResetConversationFlag());
        }

        private string GetPassengerGreeting(string commentType)
        {
            if (commentType != "regular" || (currentPassenger.passengerType != PassengerType.Business && currentPassenger.passengerType != PassengerType.Regular))
            {
                return "";
            }

            return currentPassenger.gender == Gender.Male ? "" : "";
        }

        private bool ShouldUseFlirtyDialogue()
        {
            bool isOppositeGender = playerGender != currentPassenger.gender;
            return (currentPassenger.passengerType == PassengerType.Party) ||
                   (isOppositeGender && currentPassenger.passengerType != PassengerType.Silent);
        }

        private void ShowVehicleComment(string greeting, string vehicleName, bool isHighEndVehicle, float effectiveEnginePower)
        {
            string vehicleComment = isHighEndVehicle && !string.IsNullOrEmpty(vehicleName) ?
                currentPassenger.passengerType switch
                {
                    PassengerType.Business => playerGender == Gender.Male ?
                        $"{greeting}This {vehicleName} is perfect for my meetings, man!" :
                        $"{greeting}This {vehicleName} is ideal for work, dear!",
                    PassengerType.Tourist => playerGender == Gender.Male ?
                        $"Wow, a {vehicleName}? This ride’s a trip highlight, dude!" :
                        $"A {vehicleName}? Loving this ride, hon!",
                    PassengerType.Party => playerGender == Gender.Male ?
                        $"Yo, a {vehicleName}? This car’s fire, bro!" :
                        $"Hey, a {vehicleName}? Total vibes, love!",
                    PassengerType.Silent => $"*nods approvingly at the {vehicleName}*",
                    _ => playerGender == Gender.Male ?
                        $"{greeting}Nice {vehicleName}, feels special, man!" :
                        $"{greeting}Great {vehicleName}, love the vibe, dear!"
                } :
                currentVehicleCategory switch
                {
                    VehicleCategory.Luxury => currentPassenger.passengerType == PassengerType.Business ?
                        playerGender == Gender.Male ? $"{greeting}This car’s upscale, bro!" :
                        $"{greeting}So luxurious, hon!" : "Pure luxury ride!",
                    VehicleCategory.Economy => currentPassenger.passengerType == PassengerType.Tourist ?
                        effectiveEnginePower < 40f ? "Vibe’s cool but it’s slow!" :
                        "This car’s got charm!" :
                        effectiveEnginePower < 40f ? $"{greeting}We’re crawling, man!" :
                        $"{greeting}Hope it holds up, dear!",
                    VehicleCategory.Performance => currentPassenger.passengerType == PassengerType.Party ?
                        effectiveEnginePower > 80f ? "This car’s a beast, awesome!" :
                        "Cool ride, crank it up!" : "Feels fast just sitting here!",
                    _ => autoParkSupported ? $"{greeting}Solid car, neat tech!" :
                        $"{greeting}Good car, gets it done."
                };

            ShowNotification($"{currentPassenger.passengerName}: \"{vehicleComment}\"", NotificationType.Info, 15f);
            conversationShown = true;
            conversationCount++;

            if (currentVehicleCategory == VehicleCategory.Luxury || currentVehicleCategory == VehicleCategory.Performance || autoParkSupported)
            {
                currentPassenger.positiveInteractions += (int)1f;
            }
            else if (currentVehicleCategory == VehicleCategory.Economy || effectiveEnginePower < 40f)
            {
                currentPassenger.negativeInteractions += (int)0.5f;
            }

            MelonCoroutines.Start(ResetConversationFlag());
        }

        private void ShowParkingComment(string greeting, string vehicleName, bool isHighEndVehicle)
        {
            string parkingComment = isHighEndVehicle && !string.IsNullOrEmpty(vehicleName) ?
                currentPassenger.passengerType switch
                {
                    PassengerType.Business => playerGender == Gender.Male ?
                        $"{greeting}Auto-parking in a {vehicleName}? Efficient, man!" :
                        $"{greeting}That {vehicleName} parks itself? Nice, dear!",
                    PassengerType.Tourist => playerGender == Gender.Male ?
                        $"A {vehicleName} that parks? Sweet, dude!" :
                        $"Self-parking {vehicleName}? Cool, hon!",
                    PassengerType.Party => playerGender == Gender.Male ?
                        $"Smooth parking in a {vehicleName}, bro!" :
                        $"Nice park with that {vehicleName}, love!",
                    PassengerType.Silent => $"*small nod at the {vehicleName}’s tech*",
                    _ => playerGender == Gender.Male ?
                        $"{greeting}Great parking with this {vehicleName}, man!" :
                        $"{greeting}Smooth parking, dear!"
                } :
                currentPassenger.passengerType switch
                {
                    PassengerType.Business => playerGender == Gender.Male ?
                        $"{greeting}Efficient auto-parking, bro!" :
                        $"{greeting}Love that parking tech, hon!",
                    PassengerType.Tourist => "The car parks itself? Amazing!",
                    PassengerType.Party => playerGender == Gender.Male ?
                        "Smooth parking, didn’t jolt us, man!" :
                        "Sweet parking, kept our vibe, love!",
                    PassengerType.Silent => "*small nod at the tech*",
                    _ => playerGender == Gender.Male ?
                        $"{greeting}Great parking, man!" :
                        $"{greeting}Nice parking, dear!"
                };

            ShowNotification($"{currentPassenger.passengerName}: \"{parkingComment}\"", NotificationType.Info, 15f);
            conversationShown = true;
            conversationCount++;
            currentPassenger.positiveInteractions += (int)1f;
            MelonCoroutines.Start(ResetConversationFlag());
        }

        private string AdjustCommentType(string commentType)
        {
            int randomChance = UnityEngine.Random.Range(0, 100);

            if (commentType == "sudden_stop" && randomChance >= 90)
            {
                return "regular";
            }

            if (commentType == "speeding" && randomChance >= 70)
            {
                return "regular";
            }

            if (commentType == "smooth" && randomChance >= 40)
            {
                return "regular";
            }

            return commentType;
        }

        private void UpdatePassengerInteractions(string commentType)
        {
            if (commentType == "smooth" || (commentType == "regular" && UnityEngine.Random.Range(0, 100) < 70))
            {
                currentPassenger.positiveInteractions++;
            }
            else if (commentType == "sudden_stop" || commentType == "speeding" || commentType == "collision")
            {
                currentPassenger.negativeInteractions++;
            }
        }

        private void ShowCollisionReaction()
        {
            if (currentPassenger == null || !currentPassenger.isPickedUp || currentPassenger.collisionCount == 0)
            {
                return;
            }

            string reaction = currentPassenger.passengerType switch
            {
                PassengerType.Business => currentVehicleCategory switch
                {
                    VehicleCategory.Luxury => "This is unacceptable in a car like this!",
                    VehicleCategory.Economy => "I expected a bumpy ride, but not this!",
                    _ => "Are you serious? That's going to affect your rating!"
                },
                PassengerType.Tourist => currentVehicleCategory switch
                {
                    VehicleCategory.Luxury => "I thought fancy cars were safer!",
                    VehicleCategory.Performance => "Slow down, this isn't a race!",
                    _ => "Oh my! Is this how everyone drives here?"
                },
                PassengerType.Party => currentVehicleCategory switch
                {
                    VehicleCategory.Performance => "Whoa, epic crash! But maybe chill?",
                    _ => "Hey! Easy on the car, I'm not insured for this!"
                },
                PassengerType.Silent => currentVehicleCategory == VehicleCategory.Luxury ? "*glares in disapproval*" : "*gives you a death stare!!*",
                _ => currentVehicleCategory switch
                {
                    VehicleCategory.Luxury => "Careful! This car deserves better!",
                    VehicleCategory.Economy => "I know it’s old, but don’t break it!",
                    _ => "Hey! Careful! This isn't bumper cars!"
                }
            };

            ShowNotification($"{currentPassenger.passengerName}: \"{reaction}\"", NotificationType.Warning, 15f);
            conversationShown = true;
            conversationCount++;
            currentPassenger.negativeInteractions += 2;
            MelonCoroutines.Start(ResetConversationFlag());
        }

        private void CheckDestinationReached(VehicleController playerVehicle)
        {
            if (playerVehicle == null || currentPassenger == null)
            {
                return;
            }

            if (!currentPassenger.isPickedUp)
            {
                HandlePickup(playerVehicle);
            }
            else
            {
                HandleDropoff(playerVehicle);
            }
        }

        private void HandlePickup(VehicleController playerVehicle)
        {
            float distanceToPickup = Vector3.Distance(playerVehicle.transform.position, currentPassenger.pickupLocation);
            bool isAtPickupAddress = IsPlayerAtAddress(currentPassenger.pickupAddress.gameAddress);

            if (isAtPickupAddress || distanceToPickup < CheckDistance)
            {
                timeAtPickupAddress += Time.deltaTime;
                timeSinceLastOutsideZone = 0f;

                if (timeAtPickupAddress >= PickupConfirmTime)
                {
                    currentPassenger.isPickedUp = true;
                    currentPassenger.pickupTime = Time.time;
                    currentPassenger.collisionCount = 0;
                    currentPassenger.suddenStopCount = 0;
                    currentPassenger.totalSpeedingTime = 0f;
                    lastCollisionTime = Time.time;
                    lastSuddenStopTime = -100f;
                    ShowNotification("Passenger picked up! Drive to the destination.");

                    if (pickupCircle != null)
                    {
                        UnityEngine.Object.Destroy(pickupCircle);
                        pickupCircle = null;
                    }

                    SpawnDropoffCircle();
                    CityManager.Instance.FindBuildingAndSetGuider(currentPassenger.dropoffAddress.gameAddress, true);
                    timeAtPickupAddress = 0f;
                }
            }
            else
            {
                timeSinceLastOutsideZone += Time.deltaTime;
                if (timeSinceLastOutsideZone > 0.1f)
                {
                    timeAtPickupAddress = 0f;
                }
            }
        }
        private void HandleDropoff(VehicleController playerVehicle)
        {
            float distanceToDropoff = Vector3.Distance(playerVehicle.transform.position, currentPassenger.dropoffLocation);
            bool isAtDropoffAddress = IsPlayerAtAddress(currentPassenger.dropoffAddress.gameAddress);

            if (isAtDropoffAddress || distanceToDropoff < CheckDistance)
            {
                timeAtPickupAddress += Time.deltaTime;
                timeSinceLastOutsideZone = 0f;

                if (timeAtPickupAddress >= PickupConfirmTime)
                {
                    CompleteUberJob();
                    timeAtPickupAddress = 0f;
                }
            }
            else
            {
                timeSinceLastOutsideZone += Time.deltaTime;
                if (timeSinceLastOutsideZone > 0.1f)
                {
                    timeAtPickupAddress = 0f;
                }
            }
        }

        private void CompleteUberJob()
        {
            isCompletingJob = true;

            UpdateDriverStats();
            float tipAmount = CalculateTipAmount(currentPassenger);
            bool receivedTip = tipAmount > 0f;

            AddMoneyToPlayer(currentPassenger.fare);
            if (receivedTip)
            {
                AddMoneyToPlayer(tipAmount);
            }

            string completionMessage = $"Uber job completed! You earned ${currentPassenger.fare:F2} fare" +
                                      (receivedTip ? $"+ ${tipAmount:F2} tip!" : "") +
                                      $"\n{currentPassenger.passengerName} has arrived at {currentPassenger.dropoffAddress.DisplayName}.";
            ShowNotification(completionMessage);

            hadRegularConversation = false;
            conversationCount = 0;
            CleanupCircles();

            ratingPending = true;
            var completedPassenger = currentPassenger;
            currentPassenger = null;
            uberJobActive = false;
            lastJobCompletionTime = Time.time;

            MelonCoroutines.Start(RequestRatingAfterDelay(3f, completedPassenger));
            showUberUI = true;
            SaveDriverStats();

            isCompletingJob = false;
        }

        private void UpdateDriverStats()
        {
            driverStats.totalRides++;
            driverStats.totalEarnings += currentPassenger.fare;

            if (currentPassenger.distanceToDestination > driverStats.longestRide)
            {
                driverStats.longestRide = currentPassenger.distanceToDestination;
            }

            string neighborhood = currentPassenger.dropoffAddress.neighborhood;

            if (!driverStats.neighborhoodVisits.ContainsKey(neighborhood))
            {
                driverStats.neighborhoodVisits[neighborhood] = 0;
            }

            driverStats.neighborhoodVisits[neighborhood]++;
            var mostVisited = driverStats.neighborhoodVisits.OrderByDescending(x => x.Value).FirstOrDefault();
            driverStats.mostVisitedNeighborhood = mostVisited.Key;
        }

        private float CalculateTipAmount(UberPassenger passenger)
        {
            if (passenger == null || passenger.tipCalculated)
            {
                return 0f;
            }

            float baseTipRate = passengerTypeTipRates.GetValueOrDefault(passenger.passengerType, baselineTipRate);
            string dropoffNeighborhood = passenger.dropoffAddress.neighborhood;

            if (neighborhoodTipRates.ContainsKey(dropoffNeighborhood))
            {
                baseTipRate = (baseTipRate + neighborhoodTipRates[dropoffNeighborhood]) / 2f;
            }

            float tipChance = baseTipRate;
            List<string> tipFactors = new List<string>();

            tipChance = AdjustTipChanceForDriving(tipChance, passenger, tipFactors);
            tipChance = AdjustTipChanceForInteractions(tipChance, passenger, tipFactors);
            tipChance = AdjustTipChanceForTiming(tipChance, passenger, tipFactors);
            tipChance = AdjustTipChanceForVehicle(tipChance, passenger, tipFactors);

            bool givesTip = UnityEngine.Random.Range(0f, 1f) < Mathf.Clamp(tipChance, 0.05f, 0.95f);

            if (!givesTip)
            {
                passenger.tipCalculated = true;
                return 0f;
            }

            float baseTipPercentage = CalculateBaseTipPercentage(passenger);
            float tipAmount = passenger.fare * baseTipPercentage;
            tipAmount = Mathf.Round(tipAmount * 2) / 2;
            tipAmount = Mathf.Max(tipAmount, 1f);

            UpdateDriverTipStats(tipAmount, passenger, baseTipPercentage);

            passenger.tipAmount = tipAmount;
            passenger.tipCalculated = true;
            return tipAmount;
        }

        private float AdjustTipChanceForDriving(float tipChance, UberPassenger passenger, List<string> tipFactors)
        {
            if (passenger.collisionCount == 0 && passenger.suddenStopCount <= 1 && passenger.totalSpeedingTime < 15f)
            {
                tipChance *= tipMultiplierForGoodDriving;
                tipFactors.Add("smooth driving");
            }
            else if (passenger.collisionCount >= 2 || passenger.suddenStopCount >= 3 || passenger.totalSpeedingTime > 60f)
            {
                tipChance *= tipMultiplierForBadDriving;
            }

            return tipChance;
        }

        private float AdjustTipChanceForInteractions(float tipChance, UberPassenger passenger, List<string> tipFactors)
        {
            float conversationFactor = 1.0f;

            if (passenger.positiveInteractions > 0)
            {
                conversationFactor += 0.1f * passenger.positiveInteractions;

                if (passenger.positiveInteractions >= 2)
                {
                    tipFactors.Add("good conversation");
                }
            }

            if (passenger.negativeInteractions > 0)
            {
                conversationFactor -= 0.15f * passenger.negativeInteractions;
            }

            return tipChance * conversationFactor;
        }

        private float AdjustTipChanceForTiming(float tipChance, UberPassenger passenger, List<string> tipFactors)
        {
            if (passenger.waitTime < 60f)
            {
                tipChance *= 1.2f;
                tipFactors.Add("prompt pickup");
            }
            else if (passenger.waitTime > 180f)
            {
                tipChance *= 0.7f;
            }

            if (passenger.dropoffAddress.businessType == BusinessTypeName.JewelryStore || passenger.dropoffAddress.businessType == BusinessTypeName.ElectronicsStore)
            {
                tipChance *= 1.3f;
            }

            if (passenger.passengerType == PassengerType.Business && Time.time % 10 < 3 && passenger.collisionCount == 0)
            {
                tipChance *= 1.5f;
                tipFactors.Add("timely arrival");
            }
            else if (passenger.passengerType == PassengerType.Tourist && passenger.hadSpecialConversation)
            {
                tipChance *= 1.4f;
                tipFactors.Add("local knowledge");
            }

            int currentHour = Il2Cpp.TimeHelper.CurrentHour;

            if (currentHour >= 22 || currentHour <= 4)
            {
                tipChance *= 1.3f;
                tipFactors.Add("late night service");
            }

            if (isPeakHours)
            {
                tipChance *= 0.9f;
            }

            return tipChance;
        }

        private float AdjustTipChanceForVehicle(float tipChance, UberPassenger passenger, List<string> tipFactors)
        {
            float vehicleTipModifier = currentVehicleCategory switch
            {
                VehicleCategory.Luxury => 1.4f,
                VehicleCategory.Performance => 1.2f,
                VehicleCategory.Economy => 0.8f,
                _ => 1f
            };

            float effectiveEnginePower = enginePower ?? 45f;

            if (autoParkSupported && passenger.suddenStopCount < 2)
            {
                vehicleTipModifier *= 1.1f;
                tipFactors.Add("smooth parking");
            }

            if (effectiveEnginePower < 40f)
            {
                vehicleTipModifier *= 0.9f;
                tipFactors.Add("sluggish ride");
            }
            else if (effectiveEnginePower > 80f)
            {
                vehicleTipModifier *= 1.1f;
                tipFactors.Add("powerful ride");
            }

            if (passenger.suddenStopCount > 0)
            {
                var brakes = GetPlayerVehicle()?.gameObject?.GetComponent<Il2CppNWH.VehiclePhysics2.Brakes>();

                if (brakes != null && brakes._brakeInput >= 0.6f)
                {
                    vehicleTipModifier *= 0.95f;
                    tipFactors.Add("hard braking");
                }
            }

            if (vehicleTipModifier > 1f)
            {
                tipFactors.Add("nice car");
            }
            else if (vehicleTipModifier < 1f)
            {
                tipFactors.Add("budget ride");
            }

            return tipChance * vehicleTipModifier;
        }

        private float CalculateBaseTipPercentage(UberPassenger passenger)
        {
            float conversationFactor = 1.0f;

            if (passenger.positiveInteractions > 0)
            {
                conversationFactor += 0.1f * passenger.positiveInteractions;
            }

            if (passenger.negativeInteractions > 0)
            {
                conversationFactor -= 0.15f * passenger.negativeInteractions;
            }

            float baseTipPercentage = UnityEngine.Random.Range(0.1f, 0.3f) * conversationFactor;

            baseTipPercentage *= currentVehicleCategory switch
            {
                VehicleCategory.Luxury => 1.3f,
                VehicleCategory.Performance => 1.1f,
                VehicleCategory.Economy => 0.9f,
                _ => 1f
            };

            if (autoParkSupported)
            {
                baseTipPercentage += 0.02f;
            }

            if (passenger.collisionCount == 0 && passenger.suddenStopCount == 0)
            {
                baseTipPercentage += 0.05f;
            }

            if (UnityEngine.Random.Range(0, 100) < 5)
            {
                baseTipPercentage = UnityEngine.Random.Range(0.4f, 0.5f);
            }

            return Mathf.Clamp(baseTipPercentage, 0.05f, 0.5f);
        }

        private void UpdateDriverTipStats(float tipAmount, UberPassenger passenger, float baseTipPercentage)
        {
            driverStats.totalTipsEarned += tipAmount;
            driverStats.ridesWithTips++;
            driverStats.averageTipPercentage = ((driverStats.averageTipPercentage * (driverStats.ridesWithTips - 1)) + baseTipPercentage) / driverStats.ridesWithTips;

            if (tipAmount > driverStats.highestTip)
            {
                driverStats.highestTip = tipAmount;
            }

            if (!driverStats.tipsByPassengerType.ContainsKey(passenger.passengerType))
            {
                driverStats.tipsByPassengerType[passenger.passengerType] = 0f;
            }

            driverStats.tipsByPassengerType[passenger.passengerType] += tipAmount;
        }

        private void PassengerCancelRide(string reason)
        {
            if (currentPassenger != null)
            {
                driverStats.cancelledRides++;
                ShowNotification(reason, NotificationType.Error);
                uberJobActive = false;
                CleanupCircles();
                currentPassenger = null;
                MelonCoroutines.Start(HideUIAfterDelay(5f));
                SaveDriverStats();
            }
        }

        #endregion

        #region Utility Methods

        private Vector3? GetPositionForAddress(Il2Cpp.Address address)
        {
            if (CityManager.Instance.CityBuildingControllersDictionary.TryGetValue(address, out CityBuildingController buildingController) && buildingController != null)
            {
                if (buildingController.entranceDoors != null && buildingController.entranceDoors.Length > 0)
                {
                    BuildingEntranceDoor entranceDoor = buildingController.entranceDoors[0];
                    if (entranceDoor != null && entranceDoor.doorTransform != null)
                    {
                        Vector3 position = entranceDoor.doorTransform.position;
                        return position;
                    }
                }
                else
                {
                    MelonLogger.Warning($"No entrance doors found for address: {address}");
                }
                Vector3 buildingPos = buildingController.transform.position;
                return buildingPos;
            }
            MelonLogger.Warning($"No BuildingController found for address: {address}");
            return null;
        }

        private GameObject CreateCircleOutline(Vector3 position, float radius, float thickness, Color color)
        {
            GameObject circle = new GameObject("MissionCircleOutline");
            MeshFilter meshFilter = circle.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = circle.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            const int segments = 128;
            float innerRadius = radius - thickness;

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * 2f * Mathf.PI;
                float cosAngle = Mathf.Cos(angle);
                float sinAngle = Mathf.Sin(angle);

                float outerX = cosAngle * radius;
                float outerY = sinAngle * radius;
                float innerX = cosAngle * innerRadius;
                float innerY = sinAngle * innerRadius;

                vertices.Add(new Vector3(outerX, outerY, 0f));
                vertices.Add(new Vector3(innerX, innerY, 0f));

                float uvAngle = (float)i / segments;
                uvs.Add(new Vector2(uvAngle, 1f));
                uvs.Add(new Vector2(uvAngle, 0f));
            }

            for (int i = 0; i < segments; i++)
            {
                int outerLeft = i * 2;
                int innerLeft = outerLeft + 1;
                int outerRight = ((i + 1) % segments) * 2;
                int innerRight = outerRight + 1;

                triangles.Add(outerLeft);
                triangles.Add(innerLeft);
                triangles.Add(outerRight);

                triangles.Add(innerLeft);
                triangles.Add(innerRight);
                triangles.Add(outerRight);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;

            Shader shader = FindCircleShader();
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent") ?? Shader.Find("Sprites/Default");
            }

            Material mat = new Material(shader);
            mat.color = new Color(color.r, color.g, color.b, 0.6f);
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3100;

            meshRenderer.material = mat;
            circle.transform.position = position;
            circle.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            circle.transform.localScale = Vector3.one;

            circle.transform.SetParent(null);

            return circle;
        }

        private Shader FindCircleShader()
        {
            if (currentPassenger != null)
            {
                Il2Cpp.Address address = currentPassenger.isPickedUp ? currentPassenger.dropoffAddress.gameAddress : currentPassenger.pickupAddress.gameAddress;

                if (CityManager.Instance.CityBuildingControllersDictionary.TryGetValue(address, out CityBuildingController buildingController) && buildingController != null)
                {
                    if (buildingController.entranceDoors != null && buildingController.entranceDoors.Length > 0)
                    {
                        BuildingEntranceDoor entranceDoor = buildingController.entranceDoors[0];

                        if (entranceDoor != null && entranceDoor.doorTransform != null)
                        {
                            MeshRenderer doorRenderer = entranceDoor.doorTransform.GetComponent<MeshRenderer>();

                            if (doorRenderer != null && doorRenderer.material != null && doorRenderer.material.shader != null)
                            {
                                return doorRenderer.material.shader;
                            }
                        }
                    }
                }
            }

            Shader fallback = Shader.Find("Sprites/Default") ?? Shader.Find("Legacy Shaders/Transparent/Diffuse");
            return fallback;
        }

        private void SpawnPickupCircle()
        {
            Vector3? position = GetPositionForAddress(currentPassenger.pickupAddress.gameAddress);

            if (position.HasValue)
            {
                Vector3 circlePosition = position.Value + new Vector3(0, 0.1f, 0);
                pickupCircle = CreateCircleOutline(circlePosition, 9f, 0.25f, new Color(0, 1, 0, 0.3f));
            }
            else
            {
            }
        }

        private void SpawnDropoffCircle()
        {
            Vector3? position = GetPositionForAddress(currentPassenger.dropoffAddress.gameAddress);

            if (position.HasValue)
            {
                Vector3 circlePosition = position.Value + new Vector3(0, 0.1f, 0);
                dropoffCircle = CreateCircleOutline(circlePosition, 9f, 0.25f, new Color(1, 0, 0, 0.3f));
            }
            else
            {
            }
        }

        private VehicleController GetPlayerVehicle()
        {
            try
            {
                return GameManager.Instance?.selectedVehicle;
            }
            catch
            {
                return null;
            }
        }

        private void ShowNotification(string message, NotificationType notificationType = NotificationType.Info, float displayTime = 5f)
        {
            try
            {
                if (notificationUI == null)
                {
                    MelonLogger.Error("Cannot show notification: notificationUI is null");
                    return;
                }

                if (message.Contains("completed"))
                {
                    notificationType = NotificationType.Success;
                }
                else if (message.Contains("canceled"))
                {
                    notificationType = NotificationType.Error;
                }

                if (message.Contains("Request") || message.Contains("accepted"))
                {
                    displayTime = 15f;
                }

                notificationUI.ShowNotification(message, notificationType, displayTime);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to use game notification system: {ex.Message}");
            }
        }

        private void ShowDrivingNotification(string eventType, NotificationType notificationType, float displayTime)
        {
            try
            {
                if (notificationUI == null || currentPassenger == null)
                {
                    MelonLogger.Error("Cannot show driving notification: notificationUI or currentPassenger is null");
                    return;
                }

                if (!notificationMessages.ContainsKey(currentPassenger.passengerType) ||
                    !notificationMessages[currentPassenger.passengerType].ContainsKey(eventType))
                {
                    MelonLogger.Error($"No notification messages found for passengerType {currentPassenger.passengerType} and eventType {eventType}");
                    return;
                }

                var messages = notificationMessages[currentPassenger.passengerType][eventType];
                string message = messages[UnityEngine.Random.Range(0, messages.Count)].Replace("{playerName}", playerName);
                notificationUI.ShowNotification(message, notificationType, displayTime);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to show driving notification: {ex.Message}");
            }
        }

        private void AddMoneyToPlayer(float amount)
        {
            try
            {
                var transaction = new Il2Cpp.Transaction();
                Il2Cpp.Transaction.TransactionType transactionType = Il2Cpp.Transaction.TransactionType.TaxiRide;
                var dataHolder = new Il2Cpp.Transaction.DataHolder { value = amount };
                int currentDay = Il2Cpp.TimeHelper.CurrentDay;
                var nullableDay = new Il2CppSystem.Nullable<int>(currentDay);

                GameManager.ChangeMoneySafe(
                    amount,
                    transactionType,
                    dataHolder,
                    nullableDay,
                    currentPassenger?.dropoffAddress.gameAddress,
                    true
                );
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error adding money to player: {ex.Message}");
            }
        }

        private float CalculateFare(float distance)
        {
            const float baseFare = 5.0f;
            const float minutePerMeter = 0.04f;
            const float pricePerMeter = 0.2f;
            const float pricePerMinute = 0.07f;

            float estimatedTime = distance * minutePerMeter;
            float randomFactor = UnityEngine.Random.Range(0.95f, 1.05f);

            float passengerModifier = currentPassenger != null ? currentPassenger.passengerType switch
            {
                PassengerType.Business => 1.2f,
                PassengerType.Tourist => 1.1f,
                PassengerType.Party => 0.95f,
                PassengerType.Silent => 0.9f,
                _ => 1f
            } : 1f;

            float vehicleModifier = currentVehicleCategory switch
            {
                VehicleCategory.Luxury => 1.3f,
                VehicleCategory.Performance => 1.2f,
                VehicleCategory.Economy => 0.85f,
                _ => 1f
            };

            int currentHour = Il2Cpp.TimeHelper.CurrentHour;
            float timeModifier = (currentHour >= 22 || currentHour <= 4) ? 1.15f : 1f;

            float fare = (baseFare + (pricePerMeter * distance) + (pricePerMinute * estimatedTime)) *
                         randomFactor * passengerModifier * vehicleModifier * timeModifier;

            if (isPeakHours)
            {
                fare *= PeakMultiplier;
            }

            return Mathf.Max(fare, 2f);
        }

        private bool IsPlayerAtAddress(Il2Cpp.Address targetAddress)
        {
            if (lastClosestBuildingAddress != null && lastClosestBuildingAddress.Equals(targetAddress))
            {
                return true;
            }

            VehicleController playerVehicle = GetPlayerVehicle();

            if (playerVehicle != null && buildingPositions.ContainsKey(targetAddress))
            {
                float distanceToAddress = Vector3.Distance(playerVehicle.transform.position, buildingPositions[targetAddress]);

                if (distanceToAddress < CheckDistance)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateNeighborhood()
        {
            if (playerHUD != null && playerHUD._lastClosestBuilding != null)
            {
                lastClosestBuildingAddress = playerHUD._lastClosestBuilding.Address;
                currentNeighborhood = playerHUD._lastClosestBuilding.Neighbourhood.ToString();
            }
            else
            {
                currentNeighborhood = "Unknown";
            }
        }

        private void UpdateDrivingState(VehicleController playerVehicle)
        {
            if (playerVehicle == null)
            {
                ResetVehicleState();
                return;
            }

            float currentTime = Time.time;
            currentSpeed = playerVehicle.CurrentSpeed;

            var damageHandler = playerVehicle.gameObject?.GetComponent<Il2CppNWH.VehiclePhysics2.Damage.DamageHandler>();
            var speedLimiter = playerVehicle.gameObject?.GetComponent<Il2CppNWH.VehiclePhysics2.Modules.SpeedLimiter.SpeedLimiterModule>();

            UpdateVehicleType(playerVehicle);
            UpdateDrivingMetrics(playerVehicle, currentTime, damageHandler, speedLimiter);
            UpdateSmoothDriving(currentTime);
            previousSpeed = currentSpeed;
            previousTime = currentTime;
        }

        private void ResetVehicleState()
        {
            if (lastDetectedVehicle != null)
            {
                lastDetectedVehicle = null;
                lastVehicleTypeName = null;
                maxVehicleSpeed = null;
                vehiclePrice = null;
                enginePower = null;
                autoParkSupported = false;
                currentVehicleCategory = VehicleCategory.Standard;
                isCheapCar = false;
            }
        }

        private void UpdateVehicleType(VehicleController playerVehicle)
        {
            if (playerVehicle != lastDetectedVehicle || lastVehicleTypeName == null)
            {
                try
                {
                    var vehicleType = GetVehicleType(playerVehicle);

                    if (vehicleType != null)
                    {
                        string currentVehicleTypeName = vehicleType.vehicleTypeName.ToString();

                        if (currentVehicleTypeName != lastVehicleTypeName)
                        {
                            maxVehicleSpeed = Mathf.Min(vehicleType.maxSpeed, 100f);
                            vehiclePrice = vehicleType.price;
                            enginePower = vehicleType.enginePower;
                            autoParkSupported = vehicleType.autoParkSupported;
                            currentVehicleCategory = CategorizeVehicle(vehicleType);
                            isCheapCar = currentVehicleCategory == VehicleCategory.Economy;
                            lastDetectedVehicle = playerVehicle;
                            lastVehicleTypeName = currentVehicleTypeName;
                        }
                    }
                    else
                    {
                        ResetVehicleState();
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error detecting vehicle type: {ex.Message}");
                    ResetVehicleState();
                }
            }
        }

        private void UpdateDrivingMetrics(VehicleController playerVehicle, float currentTime, Il2CppNWH.VehiclePhysics2.Damage.DamageHandler damageHandler, Il2CppNWH.VehiclePhysics2.Modules.SpeedLimiter.SpeedLimiterModule speedLimiter)
        {
            float effectiveMaxSpeed = maxVehicleSpeed ?? 100f;
            float effectiveEnginePower = enginePower ?? 45f;

            float adjustedAccelerationThreshold = AccelerationThreshold * (effectiveEnginePower < 40f ? 1.2f : effectiveEnginePower > 80f ? 0.8f : 1f);
            dynamicSpeedLimit = CalculateDynamicSpeedLimit(maxVehicleSpeed, currentVehicleCategory);

            float adjustedSpeedingTolerance = isCheapCar ? 15f : currentVehicleCategory == VehicleCategory.Performance ? 8f : speedingTolerance;

            if (lastVehicleTypeName != null && lastVehicleTypeName.Contains("Honza Mimic"))
            {
                adjustedSpeedingTolerance = 20f;
            }

            if (speedLimiter != null)
            {
                hasSpeedLimiterModule = true;
                gameSpeedLimit = speedLimiter.speedLimit;
                dynamicSpeedLimit = Mathf.Min(dynamicSpeedLimit, gameSpeedLimit);
            }
            else
            {
                hasSpeedLimiterModule = false;
                gameSpeedLimit = float.MaxValue;
            }

            HandleCollisions(playerVehicle, damageHandler, currentTime);
            HandleSuddenStops(playerVehicle, currentTime, adjustedAccelerationThreshold);
            HandleSpeeding(currentTime, adjustedSpeedingTolerance);
        }

        private void HandleCollisions(VehicleController playerVehicle, Il2CppNWH.VehiclePhysics2.Damage.DamageHandler damageHandler, float currentTime)
        {
            if (damageHandler != null)
            {
                float vehicleLastCollisionTime = damageHandler.lastCollisionTime;

                if (vehicleLastCollisionTime > lastCollisionTime &&
                    currentPassenger != null)
                {
                    if (!currentPassenger.isPickedUp)
                    {
                        return;
                    }

                    // Only count collisions after pickup
                    if (currentSpeed < 2f)
                    {
                        return;
                    }

                    lastCollisionTime = vehicleLastCollisionTime;
                    isCollision = true;
                    currentPassenger.collisionCount++;
                    smoothDrivingTime = Mathf.Max(0, smoothDrivingTime - 60f);

                    float collisionPenalty = currentVehicleCategory == VehicleCategory.Luxury ? 1.5f :
                                            currentVehicleCategory == VehicleCategory.Performance ? 2f : 1f;
                    if (currentSpeed < 10f)
                    {
                        collisionPenalty *= 0.5f;
                    }

                    currentPassenger.negativeInteractions += (int)collisionPenalty;

                    if (currentPassenger.passengerType != PassengerType.Silent || UnityEngine.Random.Range(0, 100) < 80)
                    {
                        ShowCollisionReaction();
                    }
                }
                else if (isCollision && (currentTime - lastCollisionTime) > CollisionRecoveryTime)
                {
                    isCollision = false;
                }
            }
        }

        private bool IsNearPickupOrDropoff(VehicleController playerVehicle)
        {
            if (currentPassenger == null || playerVehicle == null)
            {
                return false;
            }

            float distanceToPickup = Vector3.Distance(playerVehicle.transform.position, currentPassenger.pickupLocation);
            float distanceToDropoff = currentPassenger.isPickedUp ? Vector3.Distance(playerVehicle.transform.position, currentPassenger.dropoffLocation) : float.MaxValue;
            bool isNear = distanceToPickup < CheckDistance || distanceToDropoff < CheckDistance;
            return isNear;
        }

        private void HandleSuddenStops(VehicleController playerVehicle, float currentTime, float adjustedAccelerationThreshold)
        {
            bool isSuddenStop = false;
            float deltaTime = Mathf.Min(currentTime - previousTime, 0.1f);
            float acceleration = (currentSpeed - previousSpeed) / deltaTime;

            if (previousSpeed > 10f && acceleration < -adjustedAccelerationThreshold && (previousSpeed - currentSpeed) > 5f)
            {
                if (!currentPassenger.isPickedUp)
                {
                    return;
                }

                if (playerVehicle == null)
                {
                    return;
                }

                bool isNearPickupOrDropoff = IsNearPickupOrDropoff(playerVehicle);
                if (!isNearPickupOrDropoff)
                {
                    isSuddenStop = true;
                    if (currentTime - lastSuddenStopTime >= suddenStopCooldown)
                    {
                        lastSuddenStopTime = currentTime;
                        suddenStopCooldown = SuddenStopCooldownDuration;
                        currentPassenger.suddenStopCount++;
                        float stopPenalty = currentVehicleCategory == VehicleCategory.Luxury ? 1.5f : 1f;
                        smoothDrivingTime = Mathf.Max(0, smoothDrivingTime - 20f * stopPenalty);
                        ShowDrivingNotification("sudden_stop", NotificationType.Warning, 3f);
                    }
                }
            }

            if (suddenStopCooldown > 0)
            {
                suddenStopCooldown = Mathf.Max(0, suddenStopCooldown - (currentTime - previousTime));
            }
        }

        private void HandleSpeeding(float currentTime, float adjustedSpeedingTolerance)
        {
            float effectiveSpeedLimit = dynamicSpeedLimit;
            float speedThreshold = effectiveSpeedLimit * (1f + adjustedSpeedingTolerance / 100f);
            isSpeeding = currentSpeed > speedThreshold;

            if (isSpeeding)
            {
                float speedingFactor = currentVehicleCategory == VehicleCategory.Performance ? 0.5f : 1f;
                float deltaTime = Mathf.Min(currentTime - previousTime, 0.1f);
                currentPassenger.totalSpeedingTime += deltaTime * speedingFactor;

                if (currentPassenger.totalSpeedingTime > 10f && currentTime - lastSpeedingNotificationTime >= SpeedingNotificationCooldown)
                {
                    lastSpeedingNotificationTime = currentTime;
                    ShowDrivingNotification("speeding", NotificationType.Warning, 3f);
                }
            }
            else
            {
                float deltaTime = Mathf.Min(currentTime - previousTime, 0.1f);
                currentPassenger.totalSpeedingTime = Mathf.Max(0, currentPassenger.totalSpeedingTime - (deltaTime * 0.5f));
            }
        }

        private void UpdateSmoothDriving(float currentTime)
        {
            float deltaTime = Mathf.Min(currentTime - previousTime, 0.1f);
            float acceleration = previousTime > 0 ? (currentSpeed - previousSpeed) / deltaTime : 0f;

            if (currentSpeed > 15f && Mathf.Abs(acceleration) < SmoothAccelerationThreshold && !isCollision && !isSpeeding)
            {
                smoothDrivingTime += deltaTime;
            }
            else if (currentSpeed < 5f)
            {
                smoothDrivingTime = Mathf.Max(0, smoothDrivingTime - (deltaTime * 0.2f));
            }
        }

        private VehicleType GetVehicleType(VehicleController vehicle)
        {
            if (vehicle == null || vehicle.gameObject == null || vehicle.vehicleType == null)
            {
                return null;
            }

            try
            {
                VehicleTypeName vehicleTypeName = vehicle.vehicleType.vehicleTypeName;
                var vehicleTypes = Il2CppVehicles.VehicleTypes.VehicleTypeHelper.VehicleTypes;

                if (vehicleTypes == null)
                {
                    return null;
                }

                foreach (var pair in vehicleTypes)
                {
                    if (pair.Key == vehicleTypeName)
                    {
                        return pair.Value;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error getting vehicle type: {ex.Message}");
                return null;
            }
        }

        private VehicleCategory CategorizeVehicle(VehicleType vehicleType)
        {
            if (vehicleType == null)
            {
                return VehicleCategory.Standard;
            }

            float price = vehicleType.price;
            float maxSpeed = vehicleType.maxSpeed;
            float enginePower = vehicleType.enginePower;
            bool autoParkSupported = vehicleType.autoParkSupported;

            if (price > 200000f || (enginePower > 80f && autoParkSupported))
            {
                return VehicleCategory.Luxury;
            }
            else if (maxSpeed > 80f || enginePower > 60f)
            {
                return VehicleCategory.Performance;
            }
            else if (price < 50000f || enginePower < 40f)
            {
                return VehicleCategory.Economy;
            }

            return VehicleCategory.Standard;
        }

        private float CalculateDynamicSpeedLimit(float? maxVehicleSpeed, VehicleCategory vehicleCategory)
        {
            float baseSpeedLimit = 50f;

            if (maxVehicleSpeed.HasValue)
            {
                baseSpeedLimit = Mathf.Min(maxVehicleSpeed.Value * 0.8f, 60f);
            }

            float categoryModifier = vehicleCategory switch
            {
                VehicleCategory.Luxury => 0.9f,
                VehicleCategory.Performance => 1.1f,
                VehicleCategory.Economy => 0.8f,
                _ => 1f
            };

            float neighborhoodModifier = currentNeighborhood switch
            {
                "Midtown" => 0.8f,
                "LowerManhattan" => 0.9f,
                "HellsKitchen" => 0.95f,
                _ => 1f
            };

            return baseSpeedLimit * categoryModifier * neighborhoodModifier;
        }

        private void CleanupCircles()
        {
            if (pickupCircle != null)
            {
                UnityEngine.Object.Destroy(pickupCircle);
                pickupCircle = null;
            }

            if (dropoffCircle != null)
            {
                UnityEngine.Object.Destroy(dropoffCircle);
                dropoffCircle = null;
            }
        }

        private void SaveDriverStats()
        {
            try
            {
                string saveDir = Path.Combine(MelonEnvironment.UserDataDirectory, "UberJobMod");
                string savePath = Path.Combine(saveDir, "DriverStats.json");

                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }

                string json = JsonConvert.SerializeObject(driverStats, Formatting.Indented);
                File.WriteAllText(savePath, json);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error saving driver stats: {ex.Message}");
            }
        }

        #endregion
    }
}