![AutoDocX](images/default.png)

# AutoDocX

    An automative tool that automates the manual work of compiling and executing a c/c++ program , taking its screenshot
    and adding it into a (.docx) file - usually used for c/c++ programming assignments submissions.

## Table of Contents

1. [Introduction](#introduction)
2. [Installation](#installation)
3. [Usage](#usage)
4. [Workflow](#workflow)
5. [Configuration](#configuration)
6. [Contributing](#contributing)
7. [Authors](#authors)

## **Introduction**

    The AutoDocX is a command-line tool designed to simplify the process of compiling and executing C/C++ programs and generating a .docx file with code and program output for assignment submission. It is particularly useful for students to minimize the work overhead they had to do without autodocx.

## **Installation**

1. **Prerequisites:**

   - [.NET Core SDK](https://dotnet.microsoft.com/download) must be installed.

   - **gcc compiler** must be installed, **Windows** users can download [here](https://github.com/jmeubank/tdm-gcc/releases/download/v10.3.0-tdm64-2/tdm64-gcc-10.3.0-2.exe). Just follow the setup and you would be ready to go **(remember to select 32 or 64 bit according to your Operating System).**

   - **MacOS** users can download `gcc` through `Homebrew` by running the following command on terminal:

   **incase you don't have `Homebrew` pre-installed:**

   (Please avoid `$` symbol while copying the command!)

   ```bash
    $ /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

    $ brew install gcc
   ```

   incase you have `Hombrew` installed then just run the second command `brew install gcc`.

2. **Installation Steps:**

   After the `pre-requisites` sorted out. Run the following command on your `cmd/bash`.

   ```bash

   ```

## **Usage**

After successfull installation , autodocx can be used as follows:

```bash
autodocx [SubCommands] [options]
autodocx add FileName.cpp --wordFile outputFileName.docx --mfile
```

### **SubCommands**

---

- **Add**: Subcommand to add filePath to autodocx

  ```bash
  autodocx add FileName.cpp
  ```

- **Update**: Subcommand to update an output in the wordFile, `oldFileName.cpp` is the name of file which is currently added in .docx file (filename provided at the time of using `Add`) and `newFileName.cpp` is the name of file with which you want to replace it.

  ```bash
  autodocx update OldFileName.cpp NewFileName.cpp
  ```

- **Remove**: Subcommand to remove an output in the wordFile, `FileNameToRemove.cpp` is the name of file which you gave when using `Add` command and now you want to remove it from the output file

  ```bash
  autodocx remove FileNameToRemove.cpp
  ```

### **Options**

---

- **--wordFile**: Option to add path of word file to `AutoDocX`, if not provided then considered that output is to be added in new file named default to `out.docx`,
  Otherwise if you intend to add output to an existing .docx file then make sure to specify it.
  **(Used with `Add`, `Update`, `Remove`)**

  ```bash
  autodocx add FileName.cpp --wordFile outputFile.docx
  ```

  ```bash
  autodocx update OldFileName.cpp NewFileName.cpp --wordFile inputFile.docx
  ```

  In the case of update, output would be updated in the given file.

  ```bash
  autodocx remove FileNameToRemove.cpp --wordFile inputFile.docx
  ```

- **--mfile**: Kind of flag to specify that input file depends on multiple other files.
  Sometimes a source c/c++ program file refers to other user defined libraries/source-files.
  If this flag is set, then `AutoDocX` makes sure to add those dependency files too in the output file, if its unset, then the given source file is just added.
  **(Used with `Add`, `Update`)**

  ```bash
  autodocx add FileName.cpp --wordFile outputFile.docx --mfile
  ```

  ```bash
  autodocx update OldFileName.cpp NewFileName.cpp --wordFile inputFile.docx --mfile
  ```

  In the case of update, it tells `AutoDocX` that the `NewFileName.cpp` depends on multiple files.

- **--not**: Option to specify the files to avoid adding in case of multiple file dependencies (specified with `--mfile`).
  **(Used with `Add`, `Update`)**

  ```bash
  autodocx add FileName.cpp --wordFile outputFile.docx --not "space separated names of file"
  autodocx add FileName.cpp --wordFile outputFile.docx --not "test.h test.cpp"
  ```

  You have to strictly follow this **space separated** syntax, otherwise unexpected behaviour would occur.

  ```bash
  autodocx update OldFileName.cpp NewFileName.cpp --wordFile inputFile.docx --not "space separated names of file"
  ```

  In the case of `update`, replace scenario would be like that: output of `NewFileName.cpp` would be replaced with `OldFileName.cpp` excluding the external dependencies of `NewFileName.cpp` provided with `--not`.

## **WorkFlow**

It's already been described to you that what [AutoDocX](#introduction "introduction") can do,
Here is a usual simple workflow it can handle:

Consider you have a file named **Task1.cpp** with the following code:

```c++
#include <iostream>
#include <cmath>

const int maxTerms = 5;

int main()
{
    std::cout << "--------Program to find the standard deviation of " << maxTerms << " numbers--------\n";

    double values[maxTerms];

    for (size_t i = 0; i < maxTerms; i++)
    {
        std::cout << "Enter the " << i + 1 << " value: ";
        std::cin >> values[i];
    }
    // first find the mean (average)
    double mean = 0.0;
    for (size_t i = 0; i < maxTerms; i++)
    {
        mean += values[i];
    }
    mean /= maxTerms;

    //~ standard deviation = sqrt(SUM(values[i] - mean)^2 / N (number of values))
    double stdDev = 0.0;

    for (size_t i = 0; i < maxTerms; i++)
    {
        stdDev += powf((values[i] - mean), 2);
    }
    stdDev /= (maxTerms - 1);
    stdDev = sqrtf(stdDev);

    std::cout.showpoint;
    std::cout.precision(10);
    std::cout << "Standard deviation: " << stdDev << "\n";
}
```

Your consent is to generate an output file (.docx) named `work.docx` with this code and the screenshot of its output. Here's how you can use `AutoDocX` to make it easy:

```bash
autodocx add Task1.cpp --wordFile work.docx
```

But consider that after a while you changed `Task1.cpp` for some reasons and you also want to update its output in `work.docx`, here's how you can do that:

```bash
autodocx update Task1.cpp Task1.cpp --wordFile work.docx
```

in this case both files would be same , as you want to update the same file, this would cause the `Task1.cpp` to be re-compiled and re-executed.
<br/> If you wanted to change `Task1.cpp` with let's say another file `proc.cpp` then:

```bash
autodocx update Task1.cpp proc.cpp --wordFile work.docx
```

**That's the case when the provided source file does'nt depend on any other external user defined files.**
<br/> If it does then some `Options` are used to handle it.

Consider `Task1.cpp` has the following Include Directives :

```c++
  #include <iostream>
  #include <cmath>
  #include "Array.hpp"
  #include "Arithmetic.hpp"
```

in this case `Array.hpp` and `Arithmetic.hpp` are the external user-defined dependencies of `Task1.cpp`, now the usage would be:

```bash
autodocx add Task1.cpp --wordFile work.docx --mfile
```

you only need to raise `--mfile` flag and `AutoDocX` would handle everything. It would add both files too (their code). And if you want to exclude `Arithmetic.hpp` then simply use `--not` :

```bash
autodocx add Task1.cpp --wordFile work.docx --mfile --not "Arithmetic.hpp"
```

## **Configuration**

### **Windows Screenshot Utility**

To capture screenshot on Windows OS , a third party tool is used [nircmd.exe](https://www.nirsoft.net/utils/nircmd.html "link to nircmd.exe").
<br/>
`AutoDocX` will automatically move `nircmd.exe` to `C:\nircmd` and it will also add its path to `User's environmental variables`.
<br/>
So if you want to change the location of `nircmd.exe` then also remember to change its path in user environmental variables.

### **MacOS Screenshot Utility**

MacOS by default provided `screencapture` command, which worked very well and had many options.
<br/>
Windows didn't had any thing like that by default. So,
<br/>

**Why you did this to me Windows?**

## **Contributing**

I Welcome you to contribution section, and will encourage you to contribute as you got so far reading through the readme.

### Reporting Bugs

If you encounter any issues or unexpected behavior while using `AutoDocX`, please report them on the project's [GitHub Issues](https://github.com/SHAYANZAWAR/AutoDocX/issues) page. Be sure to provide detailed information about the problem, including steps to reproduce it.

### Feature Requests

If you have ideas for new features or improvements, feel free to submit feature requests on [GitHub Issues](https://github.com/SHAYANZAWAR/AutoDocX/issues) page.

### Code Contributions

If you'd like to contribute code to the project, follow these steps:

1. Fork the project repository.
2. Create a new branch for your feature or bug fix.
3. Code it accordingly `(try to incorporate already written code, If you want to add more, then make sure its compatible with the original)`.
4. Submit a pull request to our repository.

I will review the request and merge it if it seems right.

### Communication

For questions, discussions, or help, you can discuss on [Github Discussions](https://github.com/SHAYANZAWAR/AutoDocX/discussions/).

