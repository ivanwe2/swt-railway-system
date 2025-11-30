// Dependency placeholders
IRepository<Train> _trainRepo;
IRepository<UserProfile> _userRepo;
IRepository<Reservation> _bookingRepo;
PricingService _pricingService;
BookingService _bookingService;
ProfileService _profileService;

// Localization State
string _currentLang;
Dictionary<string, Dictionary<string, string>> _locales;

// 1. Setup Dependencies
ConfigureServices();

// 2. Load Localization Data
_locales = GetLocalizationDictionary();

// 3. UI Initialization
AnsiConsole.Write(new FigletText("Rail Portal").Color(Color.Blue));

// 4. Language Selection
_currentLang = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Select Language / Choisissez la langue / Wählen Sie die Sprache / Seleccionar idioma")
        .AddChoices(new[] { "English", "Français", "Deutsch", "Español" }));

// 5. Start Application Loop
RunApplicationLoop();

// --- Local Functions ---

void ConfigureServices()
{
    _trainRepo = new JsonRepository<Train>("trains.json", t => Guid.NewGuid());
    _userRepo = new JsonRepository<UserProfile>("users.json", u => u.Id);
    _bookingRepo = new JsonRepository<Reservation>("bookings.json", r => r.Id);

    if (!_trainRepo.GetAll().Any()) SeedTrains();

    _pricingService = new PricingService();
    _bookingService = new BookingService(_bookingRepo);
    _profileService = new ProfileService(_userRepo);
}

void RunApplicationLoop()
{
    while (true)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText(T("Header")).Color(Color.Blue));
        AnsiConsole.MarkupLine($"[green]{T("Ready")}[/]");

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(T("MenuTitle"))
                .AddChoices(new[] { T("Search"), T("Profile"), T("Cart"), T("Exit") }));

        if (choice == T("Exit")) break;

        try
        {
            if (choice == T("Search")) HandleSearchAndBooking();
            else if (choice == T("Profile")) HandleProfileManagement();
            else if (choice == T("Cart")) HandleCartViewing();
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            AnsiConsole.MarkupLine("[red]An error occurred. Press any key to return to menu.[/]");
            Console.ReadKey();
        }
    }
}

void HandleSearchAndBooking()
{
    var trains = _trainRepo.GetAll().OrderBy(t => t.DepartureTime).ToList();

    var table = new Table();
    table.AddColumn("Time");
    table.AddColumn("Route");
    table.AddColumn("Base Price");

    foreach (var t in trains)
        table.AddRow(t.DepartureTime.ToShortTimeString(), $"{t.Origin} -> {t.Destination}", $"${t.BasePrice}");

    AnsiConsole.Write(table);

    if (!AnsiConsole.Confirm(T("BookQ"))) return;

    var trainChoices = trains.ToDictionary(
        t => $"{t.DepartureTime.ToShortTimeString()} | {t.Origin} -> {t.Destination} (${t.BasePrice})",
        t => t
    );

    var selectedString = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title(T("SelectTrainTitle"))
            .PageSize(10)
            .AddChoices(trainChoices.Keys));

    var selectedTrain = trainChoices[selectedString];
    AnsiConsole.MarkupLine($"[grey]Selected: {selectedTrain.Origin} -> {selectedTrain.Destination}[/]");

    var typeChoices = new Dictionary<string, TicketType>
    {
        { T("OneWay"), TicketType.OneWay },
        { T("Return"), TicketType.Return }
    };

    var selectedTypeString = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title(T("TicketTypeTitle"))
            .AddChoices(typeChoices.Keys));

    var ticketType = typeChoices[selectedTypeString];

    Passenger passenger;
    var profiles = _profileService.GetAllProfiles();
    bool useProfile = false;

    if (profiles.Any())
    {
        useProfile = AnsiConsole.Confirm(T("LoadProfileQ"));
    }

    if (useProfile)
    {
        var profileChoice = AnsiConsole.Prompt(
            new SelectionPrompt<UserProfile>()
                .Title(T("SelectProfileTitle"))
                .AddChoices(profiles)
                .UseConverter(p => $"{p.Username} (Age: {p.DefaultPassengerDetails.Age}, Card: {p.DefaultPassengerDetails.Railcard})"));

        passenger = profileChoice.DefaultPassengerDetails;
        AnsiConsole.MarkupLine($"[green]{T("ProfileLoaded")} {profileChoice.Username}![/]");
    }
    else
    {
        var age = AnsiConsole.Ask<int>(T("AgeQ"));
        var cardInput = AnsiConsole.Prompt(
            new SelectionPrompt<RailcardType>()
                .Title(T("CardQ"))
                .AddChoices(Enum.GetValues<RailcardType>()));

        passenger = new Passenger { Age = age, Railcard = cardInput };
    }

    decimal price = _pricingService.CalculatePrice(selectedTrain, passenger, ticketType);
    AnsiConsole.MarkupLine($"[bold]{T("TypeMsg")} {selectedTypeString}[/]");
    AnsiConsole.MarkupLine($"[bold yellow]{T("PriceMsg")} ${price}[/]");

    if (AnsiConsole.Confirm(T("AddCartQ")))
    {
        var res = new Reservation
        {
            Train = selectedTrain,
            Passenger = passenger,
            TicketType = ticketType,
            FinalPrice = price,
            Status = ReservationStatus.InCart
        };
        _bookingService.AddToCart(res);
        AnsiConsole.MarkupLine($"[green]{T("AddedMsg")}[/]");
    }

    Pause();
}

void HandleProfileManagement()
{
    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title(T("ProfileMenuTitle"))
            .AddChoices(new[] { T("CreateProfile"), T("EditProfile"), T("Back") }));

    if (choice == T("Back")) return;

    if (choice == T("CreateProfile"))
    {
        var name = AnsiConsole.Ask<string>(T("EnterName"));
        var age = AnsiConsole.Ask<int>(T("AgeQ"));

        var cardInput = AnsiConsole.Prompt(
            new SelectionPrompt<RailcardType>()
                .Title(T("CardQ"))
                .AddChoices(Enum.GetValues<RailcardType>()));

        _profileService.CreateProfile(name, age, cardInput);
        AnsiConsole.MarkupLine($"[green]{T("ProfileLoaded")} {name}[/]");
    }
    else if (choice == T("EditProfile"))
    {
        var profiles = _profileService.GetAllProfiles();
        if (!profiles.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No profiles found.[/]");
        }
        else
        {
            var selectedProfile = AnsiConsole.Prompt(
                new SelectionPrompt<UserProfile>()
                    .Title(T("SelectProfileTitle"))
                    .AddChoices(profiles)
                    .UseConverter(p => $"{p.Username} | Age: {p.DefaultPassengerDetails.Age} | Card: {p.DefaultPassengerDetails.Railcard}"));

            if (AnsiConsole.Confirm($"Edit {selectedProfile.Username}?"))
            {
                var newAge = AnsiConsole.Ask<int>(T("AgeQ"));
                var newCard = AnsiConsole.Prompt(
                        new SelectionPrompt<RailcardType>()
                        .Title(T("CardQ"))
                        .AddChoices(Enum.GetValues<RailcardType>()));

                selectedProfile.DefaultPassengerDetails.Age = newAge;
                selectedProfile.DefaultPassengerDetails.Railcard = newCard;
                _userRepo.Update(selectedProfile);

                AnsiConsole.MarkupLine("[green]Profile Updated![/]");
            }
        }
    }
    Pause();
}

void HandleCartViewing()
{
    var carts = _bookingService.GetMyReservations();

    if (!carts.Any())
    {
        AnsiConsole.MarkupLine("[yellow]Cart is empty.[/]");
        Pause();
        return;
    }

    var table = new Table();
    table.AddColumn("Id");
    table.AddColumn("Route");
    table.AddColumn("Type");
    table.AddColumn("Price");
    table.AddColumn("Status");

    var choices = new Dictionary<string, Reservation>();

    foreach (var c in carts)
    {
        var display = $"{c.Train.Origin}->{c.Train.Destination} ({c.TicketType}) - ${c.FinalPrice} [{c.Status}]";
        choices.Add(display, c);
        table.AddRow(c.Id.ToString().Substring(0, 4), $"{c.Train.Origin}->{c.Train.Destination}", c.TicketType.ToString(), $"${c.FinalPrice}", c.Status.ToString());
    }
    AnsiConsole.Write(table);

    var action = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title(T("CartMenuTitle"))
            .AddChoices(T("Back"), T("CancelRes"), T("ModifyRes")));

    if (action == T("Back")) return;

    var selectedString = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title(T("SelectRes"))
            .AddChoices(choices.Keys));

    var selectedRes = choices[selectedString];

    if (action == T("CancelRes"))
    {
        if (AnsiConsole.Confirm("Are you sure?"))
        {
            _bookingService.CancelReservation(selectedRes.Id);
            AnsiConsole.MarkupLine($"[red]{T("ResCancelled")}[/]");
        }
    }
    else if (action == T("ModifyRes"))
    {
        var newType = AnsiConsole.Prompt(
            new SelectionPrompt<TicketType>()
                .Title(T("TicketTypeTitle"))
                .AddChoices(TicketType.OneWay, TicketType.Return));

        decimal newPrice = _pricingService.CalculatePrice(selectedRes.Train, selectedRes.Passenger, newType);

        if (AnsiConsole.Confirm($"Update to {newType} (${newPrice})?"))
        {
            _bookingService.ModifyReservation(selectedRes.Id, newType, newPrice);
            AnsiConsole.MarkupLine($"[green]{T("ResModified")}[/]");
        }
    }
    Pause();
}

void SeedTrains()
{
    _trainRepo.Add(new Train
    {
        Origin = "Sofia",
        Destination = "Plovdiv",
        DepartureTime = DateTime.Today.AddHours(7).AddMinutes(30),
        BasePrice = 15.00m
    });

    _trainRepo.Add(new Train
    {
        Origin = "Plovdiv",
        Destination = "Sofia",
        DepartureTime = DateTime.Today.AddHours(8).AddMinutes(15),
        BasePrice = 15.00m
    });

    _trainRepo.Add(new Train
    {
        Origin = "Sofia",
        Destination = "Varna",
        DepartureTime = DateTime.Today.AddHours(10).AddMinutes(0),
        BasePrice = 32.00m
    });

    _trainRepo.Add(new Train
    {
        Origin = "Sofia",
        Destination = "Burgas",
        DepartureTime = DateTime.Today.AddHours(13).AddMinutes(45),
        BasePrice = 28.50m
    });

    _trainRepo.Add(new Train
    {
        Origin = "Ruse",
        Destination = "Sofia",
        DepartureTime = DateTime.Today.AddHours(15).AddMinutes(30),
        BasePrice = 24.00m
    });

    _trainRepo.Add(new Train
    {
        Origin = "Varna",
        Destination = "Sofia",
        DepartureTime = DateTime.Today.AddHours(17).AddMinutes(15),
        BasePrice = 32.00m
    });

    _trainRepo.Add(new Train
    {
        Origin = "Plovdiv",
        Destination = "Burgas",
        DepartureTime = DateTime.Today.AddHours(18).AddMinutes(45),
        BasePrice = 19.00m
    });

    _trainRepo.Add(new Train
    {
        Origin = "Burgas",
        Destination = "Sofia",
        DepartureTime = DateTime.Today.AddHours(22).AddMinutes(00),
        BasePrice = 35.00m
    });
}

void Pause()
{
    AnsiConsole.WriteLine("Press any key to continue...");
    Console.ReadKey();
}

string T(string key)
{
    if (_locales.ContainsKey(_currentLang) && _locales[_currentLang].ContainsKey(key))
    {
        return _locales[_currentLang][key];
    }
    return key;
}

Dictionary<string, Dictionary<string, string>> GetLocalizationDictionary()
{
    return new Dictionary<string, Dictionary<string, string>>
    {
        ["English"] = new() {
            { "Header", "Railway System" }, { "Ready", "System Ready." },
            { "MenuTitle", "Main Menu" },
            { "Search", "Search Trains" }, { "Profile", "Manage Profiles" }, { "Cart", "View My Cart" }, { "Exit", "Exit" },
            { "EnterDest", "Enter Destination:" }, { "BookQ", "Book a ticket?" }, { "AgeQ", "Enter Age:" }, { "CardQ", "Select Railcard" },
            { "PriceMsg", "Calculated Price:" }, { "AddCartQ", "Add to Cart?" }, { "AddedMsg", "Ticket added to cart!" },
            { "TicketTypeTitle", "Select Ticket Type" }, { "OneWay", "One-Way" }, { "Return", "Round Trip" }, { "TypeMsg", "Ticket Type:" },
            { "LoadProfileQ", "Load passenger details from a Profile?" }, { "SelectProfileTitle", "Select User Profile" }, { "ProfileLoaded", "Loaded details for" }, { "SelectTrainTitle", "Select a Train" },
            { "ProfileMenuTitle", "Profile Management" }, { "CreateProfile", "Create New Profile" }, { "EditProfile", "Edit/View Profiles" }, { "EnterName", "Enter Username:" },
            { "CartMenuTitle", "Your Reservations" }, { "CancelRes", "Cancel Reservation" }, { "ModifyRes", "Modify Ticket Type" }, { "Back", "Back to Main Menu" },
            { "ResCancelled", "Reservation Cancelled." }, { "ResModified", "Reservation Updated." }, { "SelectRes", "Select a reservation to manage" }
        },
        ["Français"] = new() {
            { "Header", "Système Ferroviaire" }, { "Ready", "Système Prêt." },
            { "MenuTitle", "Menu Principal" },
            { "Search", "Rechercher des trains" }, { "Profile", "Gérer les profils" }, { "Cart", "Voir mon panier" }, { "Exit", "Quitter" },
            { "EnterDest", "Entrez la destination:" }, { "BookQ", "Réserver un billet?" }, { "AgeQ", "Entrez l'âge:" }, { "CardQ", "Carte de réduction" },
            { "PriceMsg", "Prix calculé:" }, { "AddCartQ", "Ajouter au panier?" }, { "AddedMsg", "Billet ajouté!" },
            { "TicketTypeTitle", "Sélectionnez le type de billet" }, { "OneWay", "Aller simple" }, { "Return", "Aller-retour" }, { "TypeMsg", "Type de billet:" },
            { "LoadProfileQ", "Charger les détails depuis un profil ?" }, { "SelectProfileTitle", "Sélectionnez un profil utilisateur" }, { "ProfileLoaded", "Détails chargés pour" }, { "SelectTrainTitle", "Sélectionnez un train" },
            { "ProfileMenuTitle", "Gestion de profil" }, { "CreateProfile", "Créer un nouveau profil" }, { "EditProfile", "Modifier/Voir les profils" }, { "EnterName", "Entrez le nom d'utilisateur :" },
            { "CartMenuTitle", "Vos réservations" }, { "CancelRes", "Annuler la réservation" }, { "ModifyRes", "Modifier le type de billet" }, { "Back", "Retour au menu principal" },
            { "ResCancelled", "Réservation annulée." }, { "ResModified", "Réservation mise à jour." }, { "SelectRes", "Sélectionnez une réservation à gérer" }
        },
        ["Deutsch"] = new() {
            { "Header", "Eisenbahnsystem" }, { "Ready", "System Bereit." },
            { "MenuTitle", "Hauptmenü" },
            { "Search", "Züge suchen" }, { "Profile", "Profile verwalten" }, { "Cart", "Warenkorb ansehen" }, { "Exit", "Ausgang" },
            { "EnterDest", "Ziel eingeben:" }, { "BookQ", "Ein Ticket buchen?" }, { "AgeQ", "Alter eingeben:" }, { "CardQ", "Bahncard wählen" },
            { "PriceMsg", "Berechneter Preis:" }, { "AddCartQ", "In den Warenkorb?" }, { "AddedMsg", "Ticket hinzugefügt!" },
            { "TicketTypeTitle", "Ticketart wählen" }, { "OneWay", "Einfache Fahrt" }, { "Return", "Hin- und Rückfahrt" }, { "TypeMsg", "Ticketart:" },
            { "LoadProfileQ", "Fahrgastdaten aus Profil laden?" }, { "SelectProfileTitle", "Benutzerprofil auswählen" }, { "ProfileLoaded", "Details geladen für" }, { "SelectTrainTitle", "Wählen Sie einen Zug" },
            { "ProfileMenuTitle", "Profilverwaltung" }, { "CreateProfile", "Neues Profil erstellen" }, { "EditProfile", "Profile bearbeiten/anzeigen" }, { "EnterName", "Benutzername eingeben:" },
            { "CartMenuTitle", "Ihre Reservierungen" }, { "CancelRes", "Reservierung stornieren" }, { "ModifyRes", "Ticketart ändern" }, { "Back", "Zurück zum Hauptmenü" },
            { "ResCancelled", "Reservierung storniert." }, { "ResModified", "Reservierung aktualisiert." }, { "SelectRes", "Wählen Sie eine Reservierung" }
        },
        ["Español"] = new() {
            { "Header", "Sistema Ferroviario" }, { "Ready", "Sistema Listo." },
            { "MenuTitle", "Menú Principal" },
            { "Search", "Buscar Trenes" }, { "Profile", "Gestionar Perfiles" }, { "Cart", "Ver Mi Carrito" }, { "Exit", "Salir" },
            { "EnterDest", "Introduce destino:" }, { "BookQ", "¿Reservar un billete?" }, { "AgeQ", "Introduce Edad:" }, { "CardQ", "Seleccionar tarjeta" },
            { "PriceMsg", "Precio calculado:" }, { "AddCartQ", "¿Añadir al carrito?" }, { "AddedMsg", "¡Añadido al carrito!" },
            { "TicketTypeTitle", "Seleccionar tipo de billete" }, { "OneWay", "Solo ida" }, { "Return", "Ida y vuelta" }, { "TypeMsg", "Tipo de billete:" },
            { "LoadProfileQ", "¿Cargar detalles de un perfil?" }, { "SelectProfileTitle", "Seleccionar perfil de usuario" }, { "ProfileLoaded", "Detalles cargados para" }, { "SelectTrainTitle", "Seleccionar un tren" },
            { "ProfileMenuTitle", "Gestión de Perfiles" }, { "CreateProfile", "Crear Nuevo Perfil" }, { "EditProfile", "Editar/Ver Perfiles" }, { "EnterName", "Introduce Nombre de Usuario:" },
            { "CartMenuTitle", "Tus Reservas" }, { "CancelRes", "Cancelar Reserva" }, { "ModifyRes", "Modificar Tipo de Billete" }, { "Back", "Volver al Menú Principal" },
            { "ResCancelled", "Reserva Cancelada." }, { "ResModified", "Reserva Actualizada." }, { "SelectRes", "Selecciona una reserva" }
        }
    };
}