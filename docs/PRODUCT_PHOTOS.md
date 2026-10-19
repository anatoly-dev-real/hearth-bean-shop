# Product photos

The project ships with 30 original illustrations in `src/HearthBean.Web/wwwroot/images/products`, one per product (file name = product slug). A product without such a file falls back to the photo of its category (`bread.jpg`, `pastry.jpg`, `cake.jpg`, `coffee.jpg`, `seasonal.jpg`).

To replace an illustration with a real photo, overwrite its file; to give another product its own picture, save an image as `src/HearthBean.Web/wwwroot/images/products/<slug>.jpg` (create the `products` folder if it does not exist). The site picks it up automatically (`.jpg`, `.jpeg`, `.png` and `.webp` are accepted); products without a file keep the category photo. No database change is needed.

## Rules

- Landscape or square photo, about 4:3, 1200 px wide, under 250 KB (resize and compress before saving).
- Use photos with a free licence (Unsplash, Pexels) and note the author and URL for every file in [`ASSETS.md`](ASSETS.md).
- After adding files, stop the site (Ctrl+C) and start it again with `dotnet run`, because static files are indexed at build time.

## Checklist (30 products)

| Product | File name | Search query |
| --- | --- | --- |
| Country sourdough | `country-sourdough.jpg` | country sourdough bread loaf |
| Seeded rye | `seeded-rye.jpg` | seeded rye bread |
| Olive & rosemary focaccia | `olive-rosemary-focaccia.jpg` | rosemary focaccia bread |
| Whole-wheat loaf | `whole-wheat-loaf.jpg` | whole wheat bread loaf |
| Baguette | `baguette.jpg` | baguette bread |
| Milk bread | `milk-bread.jpg` | japanese milk bread loaf |
| Everything bun | `everything-bun.jpg` | seeded bread roll bun |
| Butter croissant | `croissant.jpg` | butter croissant |
| Pain au chocolat | `pain-au-chocolat.jpg` | pain au chocolat |
| Almond croissant | `almond-croissant.jpg` | almond croissant |
| Morning bun | `morning-bun.jpg` | cinnamon sugar swirl bun |
| Cardamom knot | `cardamom-knot.jpg` | cardamom bun knot |
| Blueberry scone | `blueberry-scone.jpg` | blueberry scone |
| Lemon poppy muffin | `lemon-poppy-muffin.jpg` | lemon poppy seed muffin |
| Cheese twist | `cheese-twist.jpg` | cheese twist puff pastry |
| Apple turnover | `apple-turnover.jpg` | apple turnover pastry |
| Cinnamon roll | `cinnamon-roll.jpg` | cinnamon roll icing |
| Olive oil cake | `olive-oil-cake.jpg` | olive oil citrus cake |
| Vanilla bean cake | `vanilla-bean-cake.jpg` | vanilla layer cake slice |
| Carrot cake | `carrot-cake.jpg` | carrot cake cream cheese frosting |
| Chocolate tart | `chocolate-tart.jpg` | chocolate ganache tart |
| Cheesecake slice | `cheesecake-slice.jpg` | cheesecake slice |
| Pear & almond tart | `pear-almond-tart.jpg` | pear almond tart frangipane |
| House espresso | `espresso.jpg` | espresso cup |
| Flat white | `flat-white.jpg` | flat white coffee latte art |
| Batch brew | `filter-coffee.jpg` | filter coffee pour over cup |
| House chai | `chai.jpg` | chai latte spiced tea |
| Apple galette | `apple-galette.jpg` | apple galette rustic tart |
| Pumpkin loaf | `pumpkin-loaf.jpg` | pumpkin bread loaf |
| Cranberry orange scone | `cranberry-orange-scone.jpg` | cranberry orange scone |
