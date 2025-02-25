# SnowLibrary
 A library made for all Snows Needs
 Should you, another user wish for a specific feature, then be sure to relay it to me. i can always try to add it.


# Serializer
	* SnowSerializer.Serialize<T>(T data); 
		* Serializes the given object into a string
	* [ExcludeFromSerialization] 
		* attribute for fields. it does exactly as the name emplies
	* [IncludeWithSerialization] 
		* attribute for propperties. it does exactly as the name emplies
	* [IncludeAllProperties] 
		* attribute for classes and structs. it tells the serializer to include the properties within the class or struct even if the properties do not have the [IncludeWithSerialization] attribute
	* [IncludePrivateFields] 
		* attribute for classes and structs. it tells the serializer to include all private fields within the class or struct even if the passed setting states to ignore private fields
	* the speeds of the serializing and de-serializing depends on the CPU of the machine its executed on.

# Fields/propperties supported by the serializer:
	* Standard Fields:
		* bool, byte, sbyte, char, decimal, double, float, int, uint, long, ulong, short, ushort, string, DateTime, TimeOnly, DateOnly, TimeSpan
		* class objects containing any of the above stated variables, another class object, or any field stated below
	* fields other than standard stuff:
		* list<all standard fields> 
		* array[all standard fields] (no multidimentional arrays are supported)
		* dictionary<all standard fields, all standard fields>
		* Events with no arguments or return types. it stores the refference to all the methods it has. meaning annonymous methods wont work, neither will any top level method. 
		  further, all methods must follow standard Event rules. public void EventMethod(object? o, EventArgs e) object can be replaced with the dynamic keyword to have a usable object passed through
		* Nullable<T> of any of the standard fields stated above.
		* any Enum

# StringScrambler
    * new StringScrambler(new EncrypterSetting(ScrambleConfiguration setting1, ScrambleConfiguration setting2, ScrambleConfiguration setting3, int offset, int moveNext)); 
		* settings indicate the scramble ways, offset indicates the starting index of that setting, moveNext indicates when the next setting's index is incremented'
		* ScrambleConfiguration can be I, II, III, IV, or V
		
	* StringScrambler.Encrypt(string source); encrypts the given string
	* StringScrambler.Decrypt(string source); decrypts the given encrypted string and returns the resulting decrypted string. 

# ActivatorExtra
	* ActivatorExtra.CreateInstance(Type type)
		* creates an instance of the given type
	* ActivatorExtra.CreateInstance<T>()
		* creates an instance of the given type
	
	* Works in conjunction with the attribute "DefaultConstructorArguments". it allows you to pass default arguments to the constructor of a class or struct. that is, if you have the type of a object you wish to create, but you dont know what arguments to pass to the constructor, you can use this attribute to let the method get the arguments from the attribute to use in the constructor.
	

# File Handling
	* FileManager.Write(string path, string content, bool override); 
		* appends the content at the end of the file. if override is true the target file will be cleared and the content will be appended at the very start
	* FileManager.WriteLine(string path, string content, bool override); 
		* same as above, only this always appends on a new line.
	* FileManager.Read(string path); 
		* reads all text as one single string
	* FileManager.TryRead(string path); 
		* reads all tet as one single string, if no file exists at the given path it returns null
	* FileManager.ReadLine(string path, int line); 
		* reads the specified line if it exists in the file.
	* FileManager.ReadAllLines(string path); 
		* reads all lines and returns it as a string[]
	* FileManager.CreatFile(string path, string fileNameAndExtention);
		* Creates a file at the given destination with the given name, then closes the file.
	* FileManager.ZipDirectory(string source, string archiveDestination, CompressionLevel compressionLevel = CompressionLevel.Optimal, bool overrideExistingFile = false);
		* zips the given directory and saves it at the given destination
	

# FileOutput 
	* FileOutput.RemoveReadAnomalies()
		* removes the "\r\n" from the end of the string

		
# SnowUtils
	* SnowUtils.CreateList(Type t)
		* creates a list of the given type. does not populate it with any entries
	* SnowUtils.GetDirectoryName(string path)
		* returns the directory name of the given path
	* SnowUtils.Repeat(Action action, int times)
		* repeats the given action the given amount of times
	* SnowUtils.Repeat(Action<int> action, int times)
		* repeats the given action the given amount of times, passing the current iteration as an argument
	
	* The following entries are all extention methods
	
	* SnowUtils.ReverseOrder<T>(this List<T> values)
		* reverses the order of the given list
	* SnowUtils.IsUpper(this char c)
		* returns true if the given char is uppercase
	* SnowUtils.IsLower(this char c)
		* returns true if the given char is lowercase
	* SnowUtils.IsLetter(this char c)
		* returns true if the given char is a letter
	* SnowUtils.ToUpper(this char c)
		* returns the uppercase version of the given char
	* SnowUtils.ToLower(this char c)
		* returns the lowercase version of the given char
	* SnowUtils.IsNumber(this char c)
		* returns true if the given char is a number
	* SnowUtils.Add<TKey, TValue>(this Dictionary<TKey, TValue> dict, KeyValuePair<TKey, TValue> pair)
		* adds the given key value pair to the given dictionary
	* SnowUtils.MakeString(this char[] chars)
		* returns the given char array as a string
	* SnowUtils.Partition(this IEnumerable<T> enumerable, int totalPartitions);
		* determains the most efficient way to create smaller groups of the given IEnumberable and handles upon that conclution but never goes above the max alowed partitions. if put back together into one list it retains the same order (should you handle the items from the first split list to the last)
	* SnowUtils.NextAvailable(this List<int> list);
		* returns the next available number in the given list. if the list is empty it returns 0
	* SnowUtils.NextAvailable<T>(this Dictionary<int, T> dict);
		* returns the next available key in the given dictionary. if the dictionary is empty it returns 0
	* SnowUtils.Foreach(this IEnumerable<T> enumerable, Action<T> action);
		* executes the given action for each item in the given enumerable



# Extention Methods
	* FirstCapital(this string source); 
		* converts all letters in the string to lowercase, but makes the first letter capital
	* FirstCapitalOnAllWords(this string source); 
		* converts all first letters of each word seperated with a space to a capital letter. all other letters are converted to lower case
	* Foreach<T>(this IEnumerable<T> list, Action<T> action); 
		* repeats the action for each entry of the IEnumerable (list, array)
	* ForeachFunc(this IEnumerable<T> list, Func<T, IEnumerable<T>>; 
		* repeats the action for each entry and returns a list with each treated object. (e.g. numbers.ForeachFunc(x => x += 1); would return a IEnumerable<T> where each number has recieved +1 number)

# Console Extras
	* Input.GetKey(ConsoleKey key, bool intercept, bool wait = true); 
		* gets the given key, and waits if the Wait parameter is set to true
	* ConsoleE.WriteErrorLine<T>(T Message); 
		* writes the given message in red text and a tab on a new line
	* ConsoleE.WriteError<T>(T Message); 
		* wirtes the given message in red text and a tab
	* ConsoleE.WriteWarningLine<T>(T Message); 
		* writes the given message in yellow text and a tab on a new line
	* ConsoleE.WriteWarning<T>(T Message); 
		* writes the given message in yellow text and a tab 

# Math (also exist in extention form)
	* FloorToInt(double num); 
		* floors the given double by voiding the decimals and returns the result as an int
	* FloorToInt(float num); 
		* floors the given float by voiding the decimals and returns it as an int
	* CeilingToInt(double num); 
		* raises the given double by voiding the decimals and adding 1, then returns the result as an int
	* CeilingToInt(float num); 
		* raises the given float by voiding the decimals and adding 1, then returns the result as an int

# Vector3
	* holds 3 float values to represent the 3 axis of motion in a 3D space
	* Vector3.Distance(); 
		* calculate the distance between 2 vector3 points within a straight line
	* Vector3.Random(); 
		* returns a random vector3
	* contains definitions for math operations directly on the class itself (e.g. Vector3 + Vector3)

# Vector2
	* holds 2 float values to represent the 2 axis of motion in a 2D space
	* Vector2.Distance();
		* calculate the distance between 2 Vector2 points within a straight line
	* Vector2.Random(); 
		* returns a random Vector2
	* contains definitions for math operations directly on the class itself (e.g. Vector2 + Vector2)

# TypeWorker
	* TypeWorker.FindType(string typeName, Assembly? assembly = null); 
		* if no assembly was given, it searches through all available assemblies and returns the found type. if none were found it returns null
	* TypeWorker.FindType(string typeName, string assemblyName);
		* searchest for the specified assembly, if one is found it searches through it for the type with the given name, if no type was found returns null, otherwise it reutnrs the found ype
	* TypeWorker.CastPrimitive<T>(dynamic from, T to, Assembly? targetAssembly = null, string? targetTypeName = null) 
		* casts the given object to the given target. should assembly AND typename be given, the "T to" will be ignored and the method will use the type that is found for the casting process.
	* TypeWorker.CastPrimitive<T>(dynamic from); 
		* casts the given object to the given type. 
	* TypeWorker.CastPrimitive(dynamic from, Type to);
		* casts the given object to the given type.
	* public static dynamic CastPrimitive(dynamic from, Type to, Assembly? targetAssembly = null, string? targetTypeName = null)
		* casts the given item to the given type. if the assembly and typename are given, the type will be ignored and the method will use the type that is found for the casting process.
	* TypeWorker.TryCastPrimitive<T>(from)
		* tries to cast the given object to the given type. if it fails it returns null
	* TypeWorker.TryCastPrimitive(from, Type to)
		* tries to cast the given object to the given type. if it fails it returns null
	
	
# StringWorker
	* StringWorker.FirstCapital(this string source); 
		* makes the first letter of this string a capital letter, while making all other letters lowercase
	* StringWorker.FirstCapitalOnAllWords(this string source); 
		* makes the first letter of every space seperated segment a capital letter, while making all other letters lowercase
	* StringWorker.FirstCapitalOnAllWords(this string source); 
		* makes the first letter of every space seperated segment a capital letter, while making all other letters lowercase
	* StringWorker.Base64Encode(this string source); 
		* encodes the given string to base64
	* StringWorker.Base64Decode(this string source);
		* decodes the given base64 string
	* StringWorker.StringAnimation(this string content, int delay);
		* An IEnumerable<string> that returns strings every time with a extra character using the given delay 
	* StringWorker.StringAnimationChar(this string content, int delay);
		* An IEnumerable<char> that returns the given string character by character with the given delay in milliseconds


# Other
	* foreach(int i in int) is possible with the WinterRose library
	* foreach(int i in Range) is possible with the WinterRose library


# How to install
	open visual studio
	place the contents of the library zip file inside your project files (specific directory does not matter, as long as it is ultimately in the project folder)
	rightclick the project you wish to add the library to
	go to: add
	go to: add project refference
	go to: browse
	click browse in the bottom right
	find the SnowLibrary.dll within your project files
	click 'Ok' in the bottom right
	done