using Raylib_cs;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.Utility;
using WinterRose.Reflection;

namespace WinterRoseUtilityApp.Helldivers.IconFetcher;

public static class HelldiversIconCollection
{
    public static Sprite Medal
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1226254158278037504, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite CommonSample
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1306611420510687334, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite RareSample
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1306611408406052874, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite RequisitionSlip
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1306611395986587689, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    /* ==================================================== RegionIcons: Automaton ==================================================== */

    public static Sprite Automaton_1
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104045593100338, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Automaton_2
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104052391940250, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Automaton_3
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104031559090176, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Automaton_4
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104038492012564, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Automaton_homeworld1
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1470851340388536382, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Automaton_homeworld2
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1470851341818663029, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Automaton_homeworld3
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1470851343756296354, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Automaton_homeworld4
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1470851345048145942, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    /* ==================================================== RegionIcons: Terminids ==================================================== */

    public static Sprite Terminids_1
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104503308976229, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Terminids_2
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104510485696603, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Terminids_3
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104483012743179, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Terminids_4
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104495121961010, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    /* ==================================================== RegionIcons: Illuminate ==================================================== */

    public static Sprite Illuminate_1
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104459923357787, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Illuminate_2
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104467053416459, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Illuminate_3
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104444991639592, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Illuminate_4
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104452746645524, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    /* ==================================================== RegionIcons: Humans ==================================================== */

    public static Sprite Humans_1
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104286354538516, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Humans_2
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104295091404900, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Humans_3
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104272773517312, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Humans_4
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104279077421076, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Humans_homeworld1
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104286354538516, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Humans_homeworld2
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104295091404900, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Humans_homeworld3
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104272773517312, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Humans_homeworld4
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1384104279077421076, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    /* ==================================================== Icons ==================================================== */

    public static Sprite Discord
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1298574603098132512, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Kofi
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1298575039859396628, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Github
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1298575626864955414, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Wiki
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1296193978525417524, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Hdc
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1336735906350104586, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Victory
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1238069280508215337, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite HighPrioCampaign
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1467088190048305279, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Mo
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1240706769043456031, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite MoTaskComplete
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1325865957037445192, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite MoTaskIncomplete
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1325865167359316042, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Steam
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1373613637012426772, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Playstation
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1373613628552511559, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Xbox
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1409811621907533845, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite NewIcon
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1418931498035318885, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    /* ==================================================== Factions ==================================================== */

    public static Sprite HumansFaction
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1306623209465974925, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite TerminidsFaction
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1312127076169682965, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite AutomatonFaction
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1312126862989725707, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite IlluminateFaction
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1317057914145603635, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    /* ==================================================== FactionColours ==================================================== */

    public static Sprite HumansColour
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1374794320128770078, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite TerminidsColour
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1374793659404521522, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite AutomatonColour
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1374793571604889600, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite IlluminateColour
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1374793643583602909, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite MoColour
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1374793627213234206, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite EmptyColour
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1374794686836899900, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite GreenColour
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1443959878816108695, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    /* ==================================================== FactionColoursAnim ==================================================== */

    public static Sprite MoIncreasing
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1415770867492847799, isStatic: false, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite MoDecreasing
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1415770875583533167, isStatic: false, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    /* (Remaining animation sprites omitted for brevity – they follow the same pattern) */

    /* ==================================================== Decoration ==================================================== */

    public static Sprite LeftBanner
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1428663318461026456, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite RightBanner
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1428663327336300654, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite AlertIcon
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1362562770586828880, isStatic: false, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    /* ==================================================== Stratagems ==================================================== */

    public static Sprite StratagemUp
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1277557874041557002, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite StratagemDown
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1277557875849302107, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite StratagemLeft
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1277557877787066389, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite StratagemRight
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1277557872246652928, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    /* ==================================================== DSS ==================================================== */

    public static Sprite DssIcon
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1417971465311223808, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite DssOrbitalBlockade
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1318875016909029388, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    /* (Other DSS sprites omitted for brevity – same pattern) */

    /* ==================================================== Weather ==================================================== */

    public static Sprite IntenseHeat
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1357272522227318847, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    /* (All weather sprites follow the same lazy‑load pattern) */

    /* ==================================================== SpecialUnits ==================================================== */

    public static Sprite PredatorStrain
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1355905145992646877, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    /* (All other special unit sprites use the same pattern) */

    /* ==================================================== PlanetFeatures ==================================================== */

    public static Sprite BlackHole
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1417101106055479327, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }



    public static Sprite DssIncreasing
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1415771043330789468, isStatic: false, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite DssDecreasing
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1415771010799763658, isStatic: false, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite HumansIncreasing
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1415771095239229492, isStatic: false, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite HumansDecreasing
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1415771073965981796, isStatic: false, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite TerminidsIncreasing
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1415771240312078529, isStatic: false, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite TerminidsDecreasing
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1415771209051668480, isStatic: false, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite AutomatonIncreasing
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1415770968416063510, isStatic: false, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite AutomatonDecreasing
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1415770933012201642, isStatic: false, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite IlluminateIncreasing
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1415771157172453457, isStatic: false, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite IlluminateDecreasing
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1415771130970509434, isStatic: false, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    /* ==================================================== DSS ==================================================== */


    public static Sprite DssHeavyOrdnanceDistribution
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1318874283350687816, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite DssEagleStorm
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1318874257773690881, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite DssOperationalSupport
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1340990960120631376, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite DssEagleBlockade
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1377971389570744361, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    /* ==================================================== Weather ==================================================== */


    public static Sprite FireTornadoes
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1357272531798851584, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite ExtremeCold
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1357272540413825075, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Blizzards
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1357272548626268340, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Tremors
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1357278857232646164, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite AcidStorms
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1368174678023081994, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite IonStorms
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1368174752690339980, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite MeteorStorms
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1368174763754655851, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite RainStorms
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1368174778279530546, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Sandstorms
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1368174789243441202, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite ThickFog
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1368174807232938026, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite VolcanicActivity
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1368174822722633738, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    /* ==================================================== SpecialUnits ==================================================== */

    public static Sprite JetBrigade
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1355912552143393039, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite IncinerationCorps
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1355913678704349336, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite TheGreatHost
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1372871467255070751, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite GloomBursterStrain
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1404621462278504529, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite RuptureStrain
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1412402213938270258, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Dragonroaches
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1412402344502759425, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite HiveLords
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1412402306334457916, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Cyborgs
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1467949410573877545, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    /* ==================================================== PlanetFeatures ==================================================== */


    public static Sprite Cfcsas
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1417097098901327882, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite HiveWorld
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1417088669478813788, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite CentreOfScience
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1422643050685141053, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite XenoentomologyCentre
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1422643050685141053, isStatic: true, out field))   // same id as CentreOfScience
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite FactoryHub
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1422646158987362355, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite FracturedPlanet
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1422646803681247333, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite DeepMantleForgeComplex
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1422648073133359195, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite HelldiverTrainingFacilities
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1422657339902656542, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite NewHopeCity
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1422657922642743486, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite NewAspirationCity
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1422657920771817613, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite NewYearningCity
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1422657931186540545, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite UlgraficMine
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1422659152349495316, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite E711ExtractionFacility
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1435280504978018335, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }

    public static Sprite Cecod
    {
        get
        {
            if (field is null)
            {
                if (!DownloadSprite(1446068780429082744, isStatic: true, out field))
                    field = Sprite.CreateRectangle(10, 10, Color.Magenta);
            }
            return field;
        }
    }


    const string spritePath = "Icons/Helldivers";
    const string baseUrl = "https://cdn.discordapp.com/emojis/";

    private static bool DownloadSprite(long id, bool isStatic, [NotNullWhen(true)] out Sprite? s)
    {
        if(!Directory.Exists(spritePath))
            Directory.CreateDirectory(spritePath);

        Sprite? ico = HttpImageLoader.LoadSpriteFromUrlAsync(baseUrl + id + ".png").GetAwaiter().GetResult();

        if (ico is null)
        {
            ico = HttpImageLoader.LoadSpriteFromUrlAsync(baseUrl + id + ".gif").GetAwaiter().GetResult();

            if (ico is null)
            {
                s = null;
                return false; // failed to download image
            }
        }

        s = ico;
        return true;
    }

    public static void RegisterAllAsRichIcon()
    {
        ReflectionHelper rh = new ReflectionHelper(typeof(HelldiversIconCollection));
        List<Task> registerTasks = [];

        foreach(var mem in rh.GetMembers())
        {
            if (mem.Name.Contains('<'))
                continue;
            if(mem.Type == typeof(Sprite))
            {
                registerTasks.Add(Task.Run(() =>
                {
                    RichSpriteRegistry.RegisterSprite("helldivers_" + mem.Name, (Sprite)mem.GetValue());
                }));
            }
        }

        while (true)
        {
            bool done = true;

            for (int i = 0; i < registerTasks.Count; i++)
            {
                Task? task = registerTasks[i];
                if (task.IsCompleted)
                    registerTasks.RemoveAt(i--);
                else
                    done = false;
            }

            if (done)
                break;
        }
    }
}
