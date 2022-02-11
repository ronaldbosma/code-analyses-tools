const string file = @"C:\repos\foo\big-file.sql";
const string textToSplitOn = "GO";

const string outputFileFormat = @"C:\repos\foo\split-file-{0}.sql";
int counter = 1;


using var reader = new StreamReader(file);


var outputFile = string.Format(outputFileFormat, counter++);
var writer = new StreamWriter(outputFile);

while (!reader.EndOfStream)
{
    var line = reader.ReadLine();
    if (line != null)
    {
        if (line.Trim().Equals(textToSplitOn, StringComparison.OrdinalIgnoreCase))
        {
            writer.Close();

            outputFile = string.Format(outputFileFormat, counter++);
            writer = new StreamWriter(outputFile);
        }

        writer.WriteLine(line);
    }
}

writer.Close();