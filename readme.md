# Phonetic Kanji Analysis
There once was a really great article about predicting readings of 形声文字 kanji, but it's gone now along with all the associated datasets. This project attempts to reproduce the results. It provides a few datasets as well as the tool which generated them.

## Why should I care about this?
Simply put, there are a large number of kanji which have only one 音読み reading, and where that reading is indicated by some portion of the character, referred to as the **phonetic component**. By learning the readings for these components, it's possible to guess -- often with perfect accuracy -- how a kanji will be read in a given word. For more information, go [here](https://morg.systems/Kanji-with-a-semantic-and-phonetic-component).

## How do I read the data?
Each file consists of lines representing phonetic series, each of which contains  five fields separated by tabs (`\t`). 

- **Component**
	- The phonetic component that identifies the series. For example, the phonetic component of 協 and 恊 is 劦.
- **Kanji Coverage**
	- The percentage of the characters in this series that share the component's reading. For example, 昆 has a kanji coverage of 87%, since 昆 混 崑 棍 焜 菎 鯤 all share the reading コン, but 箟 does not. If a series has 100% kanji coverage, every character will include a component reading.
- **Readings Coverage**
	- The odds that a kanji in the series will be read with its component reading *in a given word*. For example, while the 末 series has perfect kanji coverage (抹 末 沫 秣 茉 靺 all have a マツ reading), they also have other readings which diminish the confidence with which one can use the component reading to guess a word's pronunciation. If a series has 100% kanji coverage, each character *only* has the component reading.
- **Kanji in series**
	- All kanji included in this series, separated by spaces.
- **Readings distribution**
	- All readings associated with the kanji in this set, in the form `[reading, occurrences]`. For example, the 及 series contains `[キュウ, 7], [ソウ, 1]`; this means that the reading キュウ occurs in 7 kanji in the series, while ソウ appears only once.

The lines are ordered first by **kanji coverage**, then by **series size**, and finally by **readings coverage**. 
## Where can I download the datasets?
Download the `.tsv` files from the release page. The filenames indicate the parameters that were used to create them, e.g:
- `minK=80` Only include phonetic series with at least 80% kanji coverage
- `minR=50` Only include phonetic series with at least 50% readings coverage
- `minC=5` Only include phonetic series containing at least 5 kanji

### Running the project
Download the latest release [here](https://github.com/jacobalbano/PhoneticKanjiAnalysis/releases). The `.bin` files are very important.

Pass `--help` to your commandline to see the parameters available.

### Running from source
Download the `.bin` files and place them in your working directory.

Alternately, if you want to expand the (minimal) set of data captured by these files, you can download both `kanjidic2.xml` and the entire KanjiVG repository and place them in your working directory so the project can build them when you run.
