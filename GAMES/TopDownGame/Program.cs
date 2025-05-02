using Microsoft.Xna.Framework;
using System;
using TopDownGame.Inventories;
using TopDownGame.Items;
using WinterRose.Monogame;
using WinterRose.Monogame.Weapons;
using WinterRose.Serialization;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Reflection;
using WinterRose.WinterForgeSerializing;
using System.IO;
using WinterRose.WinterForgeSerializing.Formatting;
using System.Diagnostics;
using WinterRose.WinterForgeSerializing.Workers;

//using Stream human = File.OpenRead("staticCallHuamn.txt");
//using Stream opcodes = File.Open("staticCallOpcodes.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);

//var parser = new HumanReadableParser();
//parser.Parse(human, opcodes);
//opcodes.Seek(0, SeekOrigin.Begin);

//var instr = InstructionParser.ParseOpcodes(opcodes, 20);

//var exec = new InstructionExecutor();
//var result = exec.Execute(instr);


using var game = new TopDownGame.Game1();
game.Run();