# LM2L
(Luigi's Mansion 2L(like Tool, but with a stupid pun))
Janky program for extracting files from data/dict in LM2 and extracting textures from those

#### Usecases
* Extract data/dict archive pair along with necessery decompression
* Extract textures from files within data/dict and decode them from ETC1(A4)

#### Usage for extracting data/dict pair
* You should understand that each data/dict is a pair, so you can't use one dict to extract other data(correctly that is)
1. Input path into the correct fields
2. Check options
2. Click `Go!`

##### data/dict extraction options
* `Print file info` prints readable representation of dict file into CMD or txt file
* `Output compressed/decompressed` selects how you want compressed files to be output, for reasons of not yet completely understanding the format of dict files you should keep both options enabled to diagnose incorrect decompression\extraction

#### Usage for extracting textures
* __The files you need for this are in data/dict arcives, so you'll have to extract those first__
* __Note: not all data/dict files will contain textures(obviously), so don't expect to find a Powe file in all of them!__
* Powe path should be that of a file which starts with `PÓwé`(in ASCII/cp1252). It's usually the file002
* File which contains the actual texture data usually is the one right after Powe file.
1. Input paths
2. Check options
2. Click `Go!`

##### texture extraction options
* `Number files` prefixes output filenames with a numbering that corresponds to the order in which textures are stored in the file, instead of just texture ID. Useful if you're planning to repack the textures back later
* `Extract mipmaps` will extract textures of all mipmap levels. Serves no purpose whatsoever, as those are just downscaled versions of the full texture. I made it just for compatibility sake
* `Print file info` prints readable representation of Powe sections into CMD or txt file, useful if you're planning on reinjecting edited stuff
* `Flip Y axis` flips the output image vertically, so that it looks correct(because game stores textures upside-down). Uncheck this if you don't want textures to be flipped at output(may be useful when injecting textures)

##### File000 parser usage
* __Input file000 for parsing__
* This will read all data entries in file000 and write the decoded data to a text file into the same directory
* Beware that this is kinda experimental...if you wanna know what some format identifiers are, go look in ParseFileZero in Program.cs

#### TODO:
* (in no particular order, also I give no warranty that I'm gonna do any of those things in the near future)
* Find out the full specifics of dict files
* Completely understand the model format(I know that a person on vgresource has already figured out some important stuff, but we need more specifics of the format)
* Make an extractor for models(to either wavefront or collada, or both)
* Find and reverse animation and cutscene data and make a thing to export it
* Figure out what purpose other files inside data/dict archives serve, like the file000 which usually begins with 0x01130002
* Implemet an nloc extractor (replicate functionality of RoadrunnerWMC's python script)
* Reverse layout data format. File magick seems to be `FENL`. Examples can be found in data/dict archives in romfs\art\fe\<anything that doesn't have _res postfix>
* Make options to repack the extracted data(right now it can be done, but only by hand with a hex editor)
* Turn this into a library for others to use or make a completely new project out of this when most of the research will be over, because current GUI and code organization suck


##### Unwanted features (basically embarassement section for myself)
* Messy yet somehow working code
* Inconsistencies in coding style
* Abominable GUI which is unintuitive and uncofortable to use by all means
* (Adding more salt to previous point) no CLI interface whatsoever
* Too many jokes/slang in comments
* A lot of broken grammar and typos
* Why so many useless comments!?
