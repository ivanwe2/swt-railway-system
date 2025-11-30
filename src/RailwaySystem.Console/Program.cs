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
    var trains = _trainRepo.GetAll();

    // Display Train Table
    var table = new Table();
    table.AddColumn("Origin");
    table.AddColumn("Destination");
    table.AddColumn("Time");
    table.AddColumn("Base Price");

    foreach (var t in trains)
        table.AddRow(t.Origin, t.Destination, t.DepartureTime.ToShortTimeString(), $"${t.BasePrice}");

    AnsiConsole.Write(table);

    // Booking Flow
    if (!AnsiConsole.Confirm(T("BookQ"))) return;

    // 1. Select Train (Simplified: always first for demo, or add selection prompt)
    var selectedTrain = trains.First();
    AnsiConsole.MarkupLine($"[grey]Selected: {selectedTrain.Origin} -> {selectedTrain.Destination}[/]");

    // 2. Collect Passenger Info
    var age = AnsiConsole.Ask<int>(T("AgeQ"));
    var cardInput = AnsiConsole.Prompt(
        new SelectionPrompt<RailcardType>()
            .Title(T("CardQ"))
            .AddChoices(Enum.GetValues<RailcardType>()));

    var passenger = new Passenger { Age = age, Railcard = cardInput };

    // 3. Calculate Price
    decimal price = _pricingService.CalculatePrice(selectedTrain, passenger);
    AnsiConsole.MarkupLine($"[bold yellow]{T("PriceMsg")} ${price}[/]");

    // 4. Add to Cart
    if (AnsiConsole.Confirm(T("AddCartQ")))
    {
        var res = new Reservation
        {
            Train = selectedTrain,
            Passenger = passenger,
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
    _trainRepo.Add(new Train { Origin = "London", Destination = "Paris", DepartureTime = DateTime.Today.AddHours(8), BasePrice = 100 });
    _trainRepo.Add(new Train { Origin = "London", Destination = "Berlin", DepartureTime = DateTime.Today.AddHours(12), BasePrice = 100 });
    _trainRepo.Add(new Train { Origin = "Madrid", Destination = "Barcelona", DepartureTime = DateTime.Today.AddHours(20), BasePrice = 100 });
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
                    { "PriceMsg", "Calculated Price:" }, { "AddCartQ", "Add to Cart?" }, { "AddedMsg", "Ticket added to cart!" }
                },
        ["Français"] = new() {
                    { "Header", "Système Ferroviaire" }, { "Ready", "Système Prêt." },
                    { "MenuTitle", "Menu Principal" },
                    { "Search", "Rechercher des trains" }, { "Profile", "Gérer les profils" }, { "Cart", "Voir mon panier" }, { "Exit", "Quitter" },
                    { "EnterDest", "Entrez la destination:" }, { "BookQ", "Réserver un billet?" }, { "AgeQ", "Entrez l'âge:" }, { "CardQ", "Carte de réduction" },
                    { "PriceMsg", "Prix calculé:" }, { "AddCartQ", "Ajouter au panier?" }, { "AddedMsg", "Billet ajouté!" }
                },
        ["Deutsch"] = new() {
                    { "Header", "Eisenbahnsystem" }, { "Ready", "System Bereit." },
                    { "MenuTitle", "Hauptmenü" },
                    { "Search", "Züge suchen" }, { "Profile", "Profile verwalten" }, { "Cart", "Warenkorb ansehen" }, { "Exit", "Ausgang" },
                    { "EnterDest", "Ziel eingeben:" }, { "BookQ", "Ein Ticket buchen?" }, { "AgeQ", "Alter eingeben:" }, { "CardQ", "Bahncard wählen" },
                    { "PriceMsg", "Berechneter Preis:" }, { "AddCartQ", "In den Warenkorb?" }, { "AddedMsg", "Ticket hinzugefügt!" }
                },
        ["Español"] = new() {
                    { "Header", "Sistema Ferroviario" }, { "Ready", "Sistema Listo." },
                    { "MenuTitle", "Menú Principal" },
                    { "Search", "Buscar Trenes" }, { "Profile", "Gestionar Perfiles" }, { "Cart", "Ver Mi Carrito" }, { "Exit", "Salir" },
                    { "EnterDest", "Introduce destino:" }, { "BookQ", "¿Reservar un billete?" }, { "AgeQ", "Introduce Edad:" }, { "CardQ", "Seleccionar tarjeta" },
                    { "PriceMsg", "Precio calculado:" }, { "AddCartQ", "¿Añadir al carrito?" }, { "AddedMsg", "¡Añadido al carrito!" }
                }
    };
}