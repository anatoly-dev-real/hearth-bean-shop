using System.Globalization;
using HearthBean.Web.Data.Entities;

namespace HearthBean.Web.Localization;

public static class CatalogText
{
    private static readonly Dictionary<string, (string En, string Ru, string DescriptionEn, string DescriptionRu)> Products = new()
    {
        ["country-sourdough"] = ("Country sourdough", "Деревенский хлеб на закваске", "A crackly crust, open crumb and a long, gentle fermentation.", "Хрустящая корочка, пористый мякиш и долгая мягкая ферментация."),
        ["seeded-rye"] = ("Seeded rye", "Ржаной с семенами", "Dark rye with toasted sunflower, flax and a little honey.", "Тёмная рожь с подсолнечником, льном и каплей мёда."),
        ["olive-rosemary-focaccia"] = ("Olive & rosemary focaccia", "Фокачча с оливками и розмарином", "Olive oil-rich focaccia with rosemary and briny olives.", "Фокачча на оливковом масле с розмарином и солоноватыми оливками."),
        ["whole-wheat-loaf"] = ("Whole-wheat loaf", "Цельнозерновой хлеб", "A soft everyday loaf with stone-ground whole-wheat flour.", "Мягкий хлеб на каждый день из цельнозерновой муки."),
        ["baguette"] = ("Baguette", "Багет", "A slender, crisp classic with a tender centre.", "Тонкий хрустящий багет с нежным мякишем."),
        ["milk-bread"] = ("Milk bread", "Молочный хлеб", "Pillowy, lightly sweet bread made with cultured butter.", "Воздушный чуть сладкий хлеб на сливочном масле."),
        ["everything-bun"] = ("Everything bun", "Булочка со всем", "A soft roll topped with our toasted seed blend.", "Мягкая булочка с нашей смесью поджаренных семян."),
        ["croissant"] = ("Butter croissant", "Сливочный круассан", "Laminated by hand, baked until deeply golden.", "Слоёный круассан ручной работы, выпеченный до золотистой корочки."),
        ["pain-au-chocolat"] = ("Pain au chocolat", "Шоколадный круассан", "Dark chocolate wrapped in crisp, cultured-butter pastry.", "Тёмный шоколад в хрустящем тесте на сливочном масле."),
        ["almond-croissant"] = ("Almond croissant", "Миндальный круассан", "Yesterday's croissant, reborn with almond cream.", "Круассан с миндальным кремом и лепестками миндаля."),
        ["morning-bun"] = ("Morning bun", "Утренняя булочка", "A laminated swirl rolled in cinnamon sugar.", "Слоёная спираль в сахаре с корицей."),
        ["cardamom-knot"] = ("Cardamom knot", "Узелок с кардамоном", "A fragrant knot with freshly ground cardamom.", "Ароматная булочка со свежемолотым кардамоном."),
        ["blueberry-scone"] = ("Blueberry scone", "Скон с черникой", "Tender cream scone with berries and a crisp edge.", "Нежный скон со сливками, ягодами и хрустящей корочкой."),
        ["lemon-poppy-muffin"] = ("Lemon poppy muffin", "Лимонный маффин с маком", "Bright lemon zest, poppy seeds and a soft crumb.", "Яркая лимонная цедра, мак и мягкий мякиш."),
        ["cheese-twist"] = ("Cheese twist", "Сырная слойка", "Flaky pastry twisted with aged cheddar.", "Хрустящее слоёное тесто со зрелым чеддером."),
        ["apple-turnover"] = ("Apple turnover", "Слойка с яблоком", "Seasonal apples folded into buttery pastry.", "Сезонные яблоки, завёрнутые в сливочное тесто."),
        ["cinnamon-roll"] = ("Cinnamon roll", "Булочка с корицей", "Soft dough, warm cinnamon and a little cream cheese glaze.", "Мягкое тесто, тёплая корица и немного сливочной глазури."),
        ["chocolate-chip-cookie"] = ("Chocolate chip cookie", "Печенье с шоколадом", "Chewy in the middle, crisp at the edge, generously chocolatey.", "Мягкое внутри, хрустящее по краям, с большим количеством шоколада."),
        ["olive-oil-cake"] = ("Olive oil cake", "Кекс на оливковом масле", "A fragrant, tender cake with citrus and good olive oil.", "Ароматный нежный кекс с цитрусом и хорошим оливковым маслом."),
        ["vanilla-bean-cake"] = ("Vanilla bean cake", "Торт с ванилью", "Vanilla sponge layered with a light whipped cream.", "Ванильные коржи с лёгким взбитым кремом."),
        ["carrot-cake"] = ("Carrot cake", "Морковный торт", "Spiced carrot layers with tangy cream-cheese frosting.", "Пряные морковные коржи с кремом из сливочного сыра."),
        ["chocolate-tart"] = ("Chocolate tart", "Шоколадный тарт", "Silky dark chocolate ganache in a cocoa shell.", "Шёлковый ганаш из тёмного шоколада в какао-корже."),
        ["cheesecake-slice"] = ("Cheesecake slice", "Кусочек чизкейка", "A baked vanilla cheesecake with a crisp biscuit base.", "Запечённый ванильный чизкейк на хрустящей основе."),
        ["pear-almond-tart"] = ("Pear & almond tart", "Тарт с грушей и миндалём", "Ripe pear and almond frangipane in a short crust.", "Спелая груша и миндальный франжипан в песочном тесте."),
        ["espresso"] = ("House espresso", "Эспрессо", "A balanced double shot from our rotating seasonal coffee.", "Сбалансированный двойной шот сезонного кофе."),
        ["flat-white"] = ("Flat white", "Флэт уайт", "Double espresso with silky steamed milk.", "Двойной эспрессо с бархатистым молоком."),
        ["filter-coffee"] = ("Batch brew", "Фильтр-кофе", "A clean, bright cup brewed fresh throughout the day.", "Чистый яркий кофе, который мы завариваем в течение дня."),
        ["chai"] = ("House chai", "Домашний чай масала", "Black tea simmered with warming spices and milk.", "Чёрный чай, томлённый с согревающими специями и молоком."),
        ["apple-galette"] = ("Apple galette", "Яблочная галета", "A rustic free-form tart with local apples and spice.", "Домашняя открытая галета с местными яблоками и пряностями."),
        ["pumpkin-loaf"] = ("Pumpkin loaf", "Тыквенный хлеб", "A warmly spiced loaf made with roasted pumpkin.", "Пряный хлеб с запечённой тыквой."),
        ["cranberry-orange-scone"] = ("Cranberry orange scone", "Скон с клюквой и апельсином", "Tart cranberries and fresh orange in a tender scone.", "Кислая клюква и свежий апельсин в нежном сконе.")
    };

    public static string Name(Product product) => Products.TryGetValue(product.Slug, out var value)
        ? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru" ? value.Ru : value.En
        : product.Name;

    public static string Description(Product product) => Products.TryGetValue(product.Slug, out var value)
        ? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru" ? value.DescriptionRu : value.DescriptionEn
        : product.Description;

    public static string CategoryName(Category category) => category.Slug switch
    {
        "bread" => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru" ? "Хлеб" : "Bread",
        "pastries" => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru" ? "Выпечка" : "Pastries",
        "cakes" => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru" ? "Пироги и торты" : "Cakes",
        "coffee" => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru" ? "Кофе" : "Coffee",
        "seasonal" => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru" ? "Сезонное" : "Seasonal",
        _ => category.Name
    };
}