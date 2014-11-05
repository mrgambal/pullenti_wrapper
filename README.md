#PullEnti SDK wrapper#

This project is just a handy (more or less) wrapper for Pullenti SDK.

[PullEnti][pullenti] (Puller of Entities) is a fully-featured NER-library
written in C#.
It is free for non-commercial usage so just go and try it.

Code is dirty and, probably, buggy a bit but I don't really care.
Don't get me wrong - I like to write good code.
But the main reason of creation of this wrapper was the need to extract some
info from a huge bunch of articles in hurry. Definitely clear, that I didn't 
want to spend much time polishing it and making it more generic.

##Usage##

###Input###

First you need a JSON-file. Not the whatever-you-found-json-file, but specially
prepared one.
The file should contain a set of text entities where every entity takes
separate line.

```json
{title: "....", content: "......"}
{title: "....", content: "......"}
{title: "....", content: "......"}
...
```

Remember that it's not an JSON-array: no brackets and no commas between 
objects.

###Output###

After you run

```batch
.\PullEntiCLI.exe path\to\your\file.json
```

it starts parsing your articles in parallel and writing results into files
right into the same directory where your source ```filename.json``` is.
All output should be splitted to as many files as many cores your CPU have.
So if you have your source in ```D:\txt\``` and 4 cores, you'll get:

> D:\txt\0.json

> D:\txt\1.json

> D:\txt\2.json

> D:\txt\3.json

The content of output files is slightly modified. ```Title```-key lasts the
same, ```content``` goes ```text``` and new key ```structure``` appears.
It contains an array of parsed entities.

##Requirements##

This code was tested under C# 3.5+, Mono 3+.

[pullenti]: http://pullenti.ru "PullEnti project page"