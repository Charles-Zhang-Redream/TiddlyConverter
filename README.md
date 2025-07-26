# Tiddly Converter

Initial date: 2022-09-17
Last update: 2025-07-25
Version: 1.5

Tiddly Wiki is very efficient, inspirational and productive in quickly collecting ideas and concepts, but as projects grows, it soon run into performance issues due to the way Tiddlers are structured.  
It's recommended that one install no extensions besides maybe Markdown extension, so as to minimize potential hassle when migrating contents. E.g. Don't rely on Tiddly Map extension.

This tool allows one to convert Tiddly Wiki into either a single Markdown or Loose Markdown files.  

For personal use, developing this utility as a full C# project is overkill; However the benefit is we could publish it as a .exe file. More efficient way of developing this is to use [Pure/Notebook](https://github.com/pure-the-Language/Pure/). With the introduction of AoT, this is lesser a concern.

**Features**

* Converst JSON to MD/KMD Style
* Converts Tiddly syntax to MD style (e.g. **bold**, *italic*, etc.)
* (PENDING) Preserves code blocks and MD style contents

Formatting details:

* Removes "Draft" tiddlers
* Ignores tiddly icons and colors
* We use a presumed [KMD style](https://files.totalimagine.com/PDF/KnowledgeMarkdownWorkflow-Presentation_No.1_Rev.0.5.pdf) markdown notation to deal with Tags, etc. This format is not customizable outside the source code, and I do not intend to make it more complicated.
* Supports priority sorting of "Dated" items

## Usage

(Notice Tiddly Wiki can export directly from the web version so no need to use standalone editors like `TiddlyDesktop`` since we are not saving anything.)

In ***TiddlyWiki > Tools > Export All > JSON***, then use this converter to convert JSON to proper MD formats.

Command line use:

```
TiddlyConverter <Source JSON File> <Output File/Folder>
```

If `<Output File/Folder>` ends in `.md`, then it outputs a single MD file, otherwise it creates loose files in the folder.

## Todo

Formatting:

- [ ] Pending quickly auto-subheader of Tiddlers that's already written in Markdown; This can be identified with `"type": "text/x-marked"`

Issues

- [x] (Unsolved) I tried using `dotnet publish --use-current-runtime` and in csproj set `PublishReadyToRun`, `PublishSingleFile` and `PublishTrimmed` but the final ouptut is still 30+MB for a single CLI program. That's 30MB way too big.
	* (Remark) That's .Net 7. The framework is included in the final exe. With AoT, that should be smaller.

## Changelog

* v1.0.1: Initial publication.
* v1.5: Clean, aot.