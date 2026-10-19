using System.Globalization;

namespace HearthBean.Web.Localization;

public sealed class UiText
{
    private static readonly Dictionary<string, (string En, string Ru)> Values = new()
    {
        ["Brand"] = ("Hearth & Bean", "Hearth & Bean"),
        ["NavHome"] = ("Home", "Главная"), ["NavMenu"] = ("Menu", "Меню"),
        ["NavAbout"] = ("Our story", "О нас"), ["NavContact"] = ("Contact", "Контакты"),
        ["NavOrders"] = ("My orders", "Мои заказы"), ["NavProfile"] = ("Profile", "Профиль"),
        ["NavAdmin"] = ("Dashboard", "Панель"), ["NavCart"] = ("Bag", "Корзина"),
        ["SignIn"] = ("Sign in", "Войти"), ["SignOut"] = ("Sign out", "Выйти"),
        ["Register"] = ("Create account", "Регистрация"), ["Language"] = ("Language", "Язык"),
        ["FooterLine"] = ("Small-batch baking, made with patience.", "Небольшие партии, терпеливая работа и хорошее тесто."),
        ["HeroEyebrow"] = ("BAKED FRESH, EVERY MORNING", "КАЖДОЕ УТРО ИЗ СВЕЖЕЙ ПЕЧИ"),
        ["HeroTitle"] = ("A slower kind of good morning.", "Хорошее утро не терпит спешки."),
        ["HeroCopy"] = ("Naturally leavened bread, thoughtful pastries and coffee worth lingering over.", "Хлеб на натуральной закваске, выпечка с характером и кофе, ради которого хочется задержаться."),
        ["ExploreMenu"] = ("Explore the menu", "Посмотреть меню"), ["FreshToday"] = ("Fresh from our ovens", "Сегодня из печи"),
        ["ViewAll"] = ("View all", "Весь каталог"), ["Search"] = ("Search the menu", "Найти в меню"),
        ["SearchPlaceholder"] = ("Try sourdough, almond, coffee...", "Например: закваска, миндаль, кофе..."),
        ["Category"] = ("Category", "Категория"), ["AllCategories"] = ("All bakes", "Всё меню"),
        ["Sort"] = ("Sort by", "Сортировка"), ["SortFeatured"] = ("Featured", "Популярное"),
        ["SortPriceLow"] = ("Price: low to high", "Сначала дешевле"), ["SortPriceHigh"] = ("Price: high to low", "Сначала дороже"),
        ["SortNewest"] = ("Newest", "Сначала новинки"), ["ApplyFilters"] = ("Apply", "Показать"),
        ["AddToBag"] = ("Add to bag", "В корзину"), ["OutOfStock"] = ("Sold out today", "На сегодня закончилось"),
        ["Vegan"] = ("Plant-based", "Растительное"), ["StockLabel"] = ("available today", "доступно сегодня"),
        ["Price"] = ("Price", "Цена"), ["Quantity"] = ("Quantity", "Количество"),
        ["YourBag"] = ("Your bag", "Ваша корзина"), ["EmptyBag"] = ("Your bag is taking a quiet morning.", "В корзине пока тихое утро."),
        ["ContinueShopping"] = ("Back to the menu", "Вернуться в меню"), ["Subtotal"] = ("Subtotal", "Итого"),
        ["Checkout"] = ("Continue to checkout", "Перейти к оформлению"), ["Remove"] = ("Remove", "Убрать"),
        ["OrderDetails"] = ("Order details", "Детали заказа"), ["DeliveryMethod"] = ("How would you like it?", "Как вам удобнее?"),
        ["Pickup"] = ("Pick up in store", "Заберу в пекарне"), ["Delivery"] = ("Local delivery", "Доставка"),
        ["Address"] = ("Delivery address", "Адрес доставки"), ["Phone"] = ("Phone", "Телефон"),
        ["Note"] = ("Note for the bakers", "Пожелание пекарям"), ["PlaceOrder"] = ("Place order", "Оформить заказ"),
        ["NoPayment"] = ("Demo: no real payment", "Демо: реальная оплата не производится"),
        ["AboutTitle"] = ("Good bread takes the time it takes.", "Хорошему хлебу нужно столько времени, сколько нужно."),
        ["AboutCopy"] = ("We are a neighbourhood bakery built around slow fermentation, seasonal ingredients and the simple pleasure of sharing something warm.", "Мы — пекарня по соседству. Работаем с долгой ферментацией, сезонными продуктами и простым удовольствием разделить что-то тёплое."),
        ["ContactTitle"] = ("Come say hello.", "Заходите поздороваться."), ["ContactAddress"] = ("17 Baker Street, Brooklyn, NY 11201", "17 Baker Street, Brooklyn, NY 11201"),
        ["Hours"] = ("Tue–Sun · 7:00 am – 4:00 pm", "Вт–Вс · 7:00–16:00"), ["DemoNotice"] = ("A portfolio project. All names, addresses and orders are fictional.", "Портфолио-проект. Все имена, адреса и заказы вымышлены."),
        ["AboutProject"] = ("About this project", "О проекте"), ["NoProducts"] = ("Nothing on the tray yet. Try another search.", "Пока ничего не нашлось. Попробуйте другой запрос."),
        ["Previous"] = ("Previous", "Назад"), ["Next"] = ("Next", "Дальше"), ["SignInTitle"] = ("Welcome back", "С возвращением"),
        ["Email"] = ("Email address", "Электронная почта"), ["Password"] = ("Password", "Пароль"),
        ["FullName"] = ("Full name", "Имя и фамилия"), ["ConfirmPassword"] = ("Confirm password", "Повторите пароль"),
        ["CreateAccount"] = ("Create your account", "Создать аккаунт"), ["ProfileSaved"] = ("Your profile was saved.", "Профиль сохранён."),
        ["OrderPlaced"] = ("Your order is in. Thank you!", "Заказ принят. Спасибо!"), ["OrderHistory"] = ("Order history", "История заказов"),
        ["OrderNumber"] = ("Order", "Заказ"), ["Date"] = ("Date", "Дата"), ["Status"] = ("Status", "Статус"),
        ["Total"] = ("Total", "Сумма"), ["CancelOrder"] = ("Cancel order", "Отменить заказ"), ["AdminTitle"] = ("Bakery overview", "Пекарня сегодня"),
        ["Products"] = ("Products", "Товары"), ["Customers"] = ("Customers", "Клиенты"), ["Reports"] = ("Reports", "Отчёты"),
        ["Orders"] = ("Orders", "Заказы"), ["Save"] = ("Save", "Сохранить"), ["Back"] = ("Back", "Назад"),
        ["Error"] = ("Something went wrong.", "Что-то пошло не так."), ["Forbidden"] = ("You do not have permission to view this page.", "У вас нет доступа к этой странице."),
        ["NotFound"] = ("We could not find that page.", "Эта страница не найдена."), ["CategoryBread"] = ("Bread", "Хлеб"),
        ["CategoryPastries"] = ("Pastries", "Выпечка"), ["CategoryCakes"] = ("Cakes", "Пироги и торты"),
        ["CategoryCoffee"] = ("Coffee", "Кофе"), ["CategorySeasonal"] = ("Seasonal", "Сезонное")
    };

    public string this[string key] => Values.TryGetValue(key, out var value)
        ? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru" ? value.Ru : value.En
        : key;
}