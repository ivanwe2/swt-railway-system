// Dependency placeholders
IRepository<Train> _trainRepo;
IRepository<UserProfile> _userRepo;
IRepository<Reservation> _bookingRepo;
PricingService _pricingService;
BookingService _bookingService;
ProfileService _profileService;

// Localization 
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

void ConfigureServices()
{
    // Setup Repositories with ID selectors
    _trainRepo = new JsonRepository<Train>("trains.json", t => Guid.NewGuid());
    _userRepo = new JsonRepository<UserProfile>("users.json", u => u.Id);
    _bookingRepo = new JsonRepository<Reservation>("bookings.json", r => r.Id);

    // Seed Data if empty
    if (!_trainRepo.GetAll().Any()) SeedTrains();

    // Setup Services
    _pricingService = new PricingService(); // Uses Strategies internally
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

    var age = AnsiConsole.Ask<int>(T("AgeQ"));
    var cardInput = AnsiConsole.Prompt(
        new SelectionPrompt<RailcardType>()
            .Title(T("CardQ"))
            .AddChoices(Enum.GetValues<RailcardType>()));

    var passenger = new Passenger { Age = age, Railcard = cardInput };

    decimal price = _pricingService.CalculatePrice(selectedTrain, passenger, ticketType);

    AnsiConsole.MarkupLine($"[bold]{T("TypeMsg")} {selectedTypeString}[/]");
    AnsiConsole.MarkupLine($"[bold yellow]{T("PriceMsg")} ${price}[/]");

    // 6. Add to Cart
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
    AnsiConsole.MarkupLine($"[bold]{T("Profile")}[/]");

    var name = AnsiConsole.Ask<string>("Username:");
    var age = AnsiConsole.Ask<int>("Age:");

    _profileService.CreateProfile(name, age, RailcardType.None);

    AnsiConsole.MarkupLine($"[green]Profile for {name} created successfully.[/]");
    Pause();
}

void HandleCartViewing()
{
    var carts = _bookingService.GetMyReservations();

    if (carts.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]Cart is empty.[/]");
    }
    else
    {
        var table = new Table();
        table.AddColumn("Route");
        table.AddColumn("Price");
        table.AddColumn("Status");
        table.AddColumn("Expires");

        foreach (var c in carts)
        {
            table.AddRow(
                $"{c.Train.Origin}->{c.Train.Destination}",
                $"${c.FinalPrice}",
                c.Status.ToString(),
                c.CreatedAt.AddDays(7).ToShortDateString()
            );
        }
        AnsiConsole.Write(table);
    }
    Pause();
}

void SeedTrains()
{
    _trainRepo.Add(new Train
    {
        Origin = "Sofia",
        Destination = "Plovdiv",
        DepartureTime = DateTime.Today.AddHours(7).AddMinutes(30), // 07:30
        BasePrice = 15.00m
    });

    _trainRepo.Add(new Train
    {
        Origin = "Plovdiv",
        Destination = "Sofia",
        DepartureTime = DateTime.Today.AddHours(8).AddMinutes(15), // 08:15
        BasePrice = 15.00m
    });

    _trainRepo.Add(new Train
    {
        Origin = "Sofia",
        Destination = "Varna",
        DepartureTime = DateTime.Today.AddHours(10).AddMinutes(0), // 10:00
        BasePrice = 32.00m
    });

    _trainRepo.Add(new Train
    {
        Origin = "Sofia",
        Destination = "Burgas",
        DepartureTime = DateTime.Today.AddHours(13).AddMinutes(45), // 13:45
        BasePrice = 28.50m
    });

    _trainRepo.Add(new Train
    {
        Origin = "Ruse",
        Destination = "Sofia",
        DepartureTime = DateTime.Today.AddHours(15).AddMinutes(30), // 15:30
        BasePrice = 24.00m
    });

    _trainRepo.Add(new Train
    {
        Origin = "Varna",
        Destination = "Sofia",
        DepartureTime = DateTime.Today.AddHours(17).AddMinutes(15), // 17:15
        BasePrice = 32.00m
    });

    _trainRepo.Add(new Train
    {
        Origin = "Plovdiv",
        Destination = "Burgas",
        DepartureTime = DateTime.Today.AddHours(18).AddMinutes(45), // 18:45
        BasePrice = 19.00m
    });

    _trainRepo.Add(new Train
    {
        Origin = "Burgas",
        Destination = "Sofia",
        DepartureTime = DateTime.Today.AddHours(22).AddMinutes(00), // 22:00 (Sleeper)
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
    return key; // Fallback
}

static Dictionary<string, Dictionary<string, string>> GetLocalizationDictionary()
{
    return new Dictionary<string, Dictionary<string, string>>
    {
        ["English"] = new() {
            { "Header", "Railway System" }, { "Ready", "System Ready." },
            { "MenuTitle", "Main Menu" },
            { "Search", "Search Trains" }, { "Profile", "Manage Profiles" }, { "Cart", "View My Cart" }, { "Exit", "Exit" },
            { "EnterDest", "Enter Destination:" }, { "BookQ", "Book a ticket?" }, { "AgeQ", "Enter Age:" }, { "CardQ", "Select Railcard" },
            { "PriceMsg", "Calculated Price:" }, { "AddCartQ", "Add to Cart?" }, { "AddedMsg", "Ticket added to cart!" },
            { "TicketTypeTitle", "Select Ticket Type" },
            { "OneWay", "One-Way" },
            { "Return", "Round Trip" },
            { "TypeMsg", "Ticket Type:" },
            { "SelectTrainTitle", "Select a Train" }
        },
        ["Français"] = new() {
            { "Header", "Système Ferroviaire" }, { "Ready", "Système Prêt." },
            { "MenuTitle", "Menu Principal" },
            { "Search", "Rechercher des trains" }, { "Profile", "Gérer les profils" }, { "Cart", "Voir mon panier" }, { "Exit", "Quitter" },
            { "EnterDest", "Entrez la destination:" }, { "BookQ", "Réserver un billet?" }, { "AgeQ", "Entrez l'âge:" }, { "CardQ", "Carte de réduction" },
            { "PriceMsg", "Prix calculé:" }, { "AddCartQ", "Ajouter au panier?" }, { "AddedMsg", "Billet ajouté!" },
            { "TicketTypeTitle", "Sélectionnez le type de billet" },
            { "OneWay", "Aller simple" },
            { "Return", "Aller-retour" },
            { "TypeMsg", "Type de billet:" },
            { "SelectTrainTitle", "Sélectionnez un train" }
        },
        ["Deutsch"] = new() {
            { "Header", "Eisenbahnsystem" }, { "Ready", "System Bereit." },
            { "MenuTitle", "Hauptmenü" },
            { "Search", "Züge suchen" }, { "Profile", "Profile verwalten" }, { "Cart", "Warenkorb ansehen" }, { "Exit", "Ausgang" },
            { "EnterDest", "Ziel eingeben:" }, { "BookQ", "Ein Ticket buchen?" }, { "AgeQ", "Alter eingeben:" }, { "CardQ", "Bahncard wählen" },
            { "PriceMsg", "Berechneter Preis:" }, { "AddCartQ", "In den Warenkorb?" }, { "AddedMsg", "Ticket hinzugefügt!" },
            { "TicketTypeTitle", "Ticketart wählen" },
            { "OneWay", "Einfache Fahrt" },
            { "Return", "Hin- und Rückfahrt" },
            { "TypeMsg", "Ticketart:" },
            { "SelectTrainTitle", "Wählen Sie einen Zug" }
        },
        ["Español"] = new() {
            { "Header", "Sistema Ferroviario" }, { "Ready", "Sistema Listo." },
            { "MenuTitle", "Menú Principal" },
            { "Search", "Buscar Trenes" }, { "Profile", "Gestionar Perfiles" }, { "Cart", "Ver Mi Carrito" }, { "Exit", "Salir" },
            { "EnterDest", "Introduce destino:" }, { "BookQ", "¿Reservar un billete?" }, { "AgeQ", "Introduce Edad:" }, { "CardQ", "Seleccionar tarjeta" },
            { "PriceMsg", "Precio calculado:" }, { "AddCartQ", "¿Añadir al carrito?" }, { "AddedMsg", "¡Añadido al carrito!" },
            { "TicketTypeTitle", "Seleccionar tipo de billete" },
            { "OneWay", "Solo ida" },
            { "Return", "Ida y vuelta" },
            { "TypeMsg", "Tipo de billete:" },
            { "SelectTrainTitle", "Seleccionar un tren" }
        }
    };
}