# waifu2x-ncnn-vulkan-GUI
![ss](https://user-images.githubusercontent.com/16046279/184076563-8d04619b-cd05-42e3-ac5b-cac03b43ce62.png)
Windows Multilingual GUI for [waifu2x-ncnn-vulkan](https://github.com/nihui/waifu2x-ncnn-vulkan), [realesrgan-ncnn-vulkan](https://github.com/xinntao/Real-ESRGAN-ncnn-vulkan), [realcugan-ncnn-vulkan](https://github.com/nihui/realcugan-ncnn-vulkan) and [realsr-ncnn-vulkan](https://github.com/nihui/realsr-ncnn-vulkan).  

This project was forked from https://github.com/f11894/waifu2x-ncnn-vulkan-GUI

## Download
Download [here](https://github.com/flavw/waifu2x-ncnn-vulkan-GUI/releases/download/2.1.1.3/waifu2x-ncnn-vulkan-GUI_2.1.1.3.zip)

## Localization
1. Localization can be done via user-editable xaml file.
2. The localization files have the name UILang._language-code_.xaml; where _language-code_ is a 5-character identifier like en-US, zh-TW, ja-JP.
3. Make a copy of one of the bundled localization files.
4. Rename the file with your target language code replacing the original.
5. Using a text editor that support UTF-8, make the following changes:
  * ```<sys:String x:Key="ResourceDictionaryName">waifu2xui-en-US</sys:String>```
  * Replace en-US with the target language code
  * All the text enclosed by the ```<sys:String>``` tags
6. About language code:
  * Make up from _ab_-_XY_
  * ab can be found [Here](http://www.loc.gov/standards/iso639-2/php/langcodes-search.php) as Alpha-2 codes
  * XY can be found [Here](https://www.iso.org/obp/ui/#search)
  * Essentially _ab_ is the language, _XY_ is the country
  * Translators: @flavw, @laikal, @martin-varela, @MartBlur, @papjul, @xperiazu21, @yoaan.
