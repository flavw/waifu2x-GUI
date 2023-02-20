# waifu2x-ncnn-vulkan-GUI
![ss](https://user-images.githubusercontent.com/16046279/184076563-8d04619b-cd05-42e3-ac5b-cac03b43ce62.png)
Multilingual GUI for [waifu2x-ncnn-vulkan](https://github.com/nihui/waifu2x-ncnn-vulkan), [realesrgan-ncnn-vulkan](https://github.com/xinntao/Real-ESRGAN-ncnn-vulkan), [realcugan-ncnn-vulkan](https://github.com/nihui/realcugan-ncnn-vulkan) and [realsr-ncnn-vulkan](https://github.com/nihui/realsr-ncnn-vulkan).  
Localization can be done via user-editable xaml file.

This project was forked from https://github.com/f11894/waifu2x-ncnn-vulkan-GUI

## Localization
1. The localization files have the name UILang._language-code_.xaml; where _language-code_ is a 5-character identifier like en-US, zh-TW, ja-JP.
2. Make a copy of one of the bundled localization files.
3. Rename the file with your target language code replacing the original.
4. Using a text editor that support UTF-8, make the following changes:
  * ```<sys:String x:Key="ResourceDictionaryName">waifu2xui-en-US</sys:String>```
  * Replace en-US with the target language code
  * All the text enclosed by the ```<sys:String>``` tags
5. About language code:
  * Make up from _ab_-_XY_
  * ab can be found [Here](http://www.loc.gov/standards/iso639-2/php/langcodes-search.php) as Alpha-2 codes
  * XY can be found [Here](https://www.iso.org/obp/ui/#search)
  * Essentially _ab_ is the language, _XY_ is the country
  Translators: @yoaan, @xperiazu21, @MartBlur, @martin-varela, @laikal, @flavw
