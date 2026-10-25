# P2 art provenance and prompts

Mode: built-in image_gen. No API/CLI fallback. Original generated creature designs; user document images were visual references only and are not used as game sprites. PNG alpha is preserved. No downloaded franchise sprites.

## Shared character prompt

Create an ORIGINAL premium hand-painted 2D chibi fantasy creature game RIG SPRITE SHEET, a single character design (no existing franchise). Transparent background. Square image, EXACT 2x2 equal cells, no lines, no text, no shadows outside subject. Each cell content centered with generous 12% transparent padding, never crossing cell boundaries. TOP LEFT: full body character 3/4 facing right, feet visible. TOP RIGHT: ONLY its detached head with ears/hat, no torso. BOTTOM LEFT: ONLY headless torso with arms and legs, neck at top, no head. BOTTOM RIGHT: detached signature tail/leaf/wing accessory for independent animation. Same scale/style/material/lighting across parts, readable polished soft outlines, dimensional painted shading, amber bounce light upper left, collectible RPG quality, expressive adorable eyes. This is usable layered Unity art, not a UI mockup.

## Design directions

Species variations: sprout-eared kitten, leaf frog, mushroom imp, vine lizard, flower hedgehog, antlered forest deer, ancient tree guardian, metal kitten, ember lion, mechanical crystal bird, coral water otter and mineral turtle. Seven form edits preserve identity while adding armor, crown, leaves and richer silhouette at the specified thresholds. All sheets use one consistent painted style.

Lobby direction: botanical fantasy observatory, turquoise and moss greens, warm gold trim, a clear central platform, layered floating islands, no lettering. Island directions: forest roots, amber volcano, silver crystal citadel, coral waterfalls, sandstone ruins; individual compositions and materials. Stone direction: exactly seven separate polished jade and gold upgrade gems, progressively elaborate silhouettes, one horizontal row, transparent background, no labels.

## Runtime processing

Character generation did not obey exact grid bounds in every image. P2ArtImporter identifies alpha-connected components, selects the largest component for each intended part, and stores sprite rectangles plus persistent convex contour data. P2SilhouetteMesh builds Unity UI geometry from that data; use the supplied pet prefabs/contour component for isolated parts. Raw sprite rectangles can overlap neighboring sheet components. PNG source pixels remain unchanged. Runtime rigs assemble the separated head/torso/accessory; full body is used for collection portraits. This is a painted cutout rig, not a hand-drawn frame-by-frame animation or a Spine skeleton.

Elemental stone variants use a code-native shader over the generated jade core, with separate vein/ripple/flame/facet/seam engraving. Five independent 7-frame paintings are not claimed.

Final saved directory: `D:/UnityProjects/PetGemBattle/Assets/PetGemBattle/Resources/P2Art/`. Each row below is a saved PNG in that directory.

## Project outputs and SHA-256

| PNG | SHA-256 |
|---|---|
| earth-turtle.png | 8b84fa986ec1aa8367cc23e85220590c764032fa1d73c79af55c1186a9050344 |
| fire-lion.png | 981a29a68aa967ed0b169f1a0a28cca011bb20609ed0739a818be311312cdbf0 |
| island-earth.png | 76e5c072ec2596bd1adb372d702e2c8c766ecd33e188291217a2225e55fcf3df |
| island-fire.png | 2ec5c5f9906b58c97334fd51d8e94faa2f7be45053c23a2888ef995c94834293 |
| island-metal.png | 3bca6096e6db71e9eff01a04aa2afe41ceccfbf45430c7e321209c2d986aef10 |
| island-water.png | 5d5836d11b6781d0ab92241e1e8663bc7d5be591bcf9641eacc1596f773d9d11 |
| island-wood.png | 41dd7b441c31d0f25fd008594b1096c2e0034ee69e56e85875ec4ebbba383f49 |
| lobby.png | 2bd3bbc95f4adc0ad34d318193c7218ea7ba2a412e1d8a161fe64b01943fe53e |
| metal-bird.png | 0123970e9d644af5af6640a89e2fc09984cc231f7c28f868f786106b50e04bd4 |
| starter-metal.png | 8daf380e9d27a0178f11bcf1d924ae793c13fcd22b7809248e4dec70ac56da95 |
| stones.png | 5355fe5bd9d8042cd2abd9c78994b024af7b0cf9d4fc0db13b7812249401031f |
| water-otter.png | 6438c60700a3743cb873219080896f394cf51de5c69b17cc3379aa6ada83e3af |
| wood-1.png | 21c79ea02ff37f31a0c7d98093c4321cd736f5254a80cecaf8aafb0cc15f2e77 |
| wood-2-form1.png | 52550311d59213ddee335172fb512435aefb5158319920d27179cd2b739f0a81 |
| wood-2.png | 13c8c3fed48045f849d3d1d83d814c72e87563e3bc1ae67f8d2793ef10f9acd8 |
| wood-3-form1.png | ee031eae788ba05f6c7b6c54fa43bdaf6b5b45e9801b2c8c4a4623af97d065af |
| wood-3.png | 6683288b1915a398dbc5608bc859bb6a736e009f159285dbb9a51adb54a25fa2 |
| wood-4-form1.png | f01a6b2692957e63f30ca027d5f401c136108340d54251a90486f3e2c7edda9d |
| wood-4.png | ceb1a85a73590e17013e9543888575023fd6585ef77595a2334e55b8463d1a71 |
| wood-5-form1.png | c7c39568913b0e01e50554c3c2b9cdfece76dd2c58821b7e47a3d8abbc725bfb |
| wood-5.png | a996504fbd38ab9dfb4aa375885be600ca4f04e482b5d761f3a8a030a9776ed3 |
| wood-6-form1.png | 09ec0ba94dc61d3b14161f8b88ff8446e36ec39e16052e0f3eff1487cf1cbc32 |
| wood-6.png | 001f69a8148f6b301603e2446f76f112131bd7384ecfa2749860d0b8ee5fb0b0 |
| wood-7-form1.png | 492e351a7ba89ac9a6f2d3a82abc1be2576a338db6c4a767a278fed7d0a69fc9 |
| wood-7-form2.png | 39950453a68aa969957e0a814088164a1a5a2070bae31195f27ce98b359831c3 |
| wood-7.png | c8cd687676730ebadb95e8a2ed50346053333f1ef786719ff0e349a1e5fe02f6 |
