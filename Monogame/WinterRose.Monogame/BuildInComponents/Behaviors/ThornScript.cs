using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FileManagement;
using WinterRose.Monogame.WinterThornPort;
using WinterRose.WinterThornScripting;
using WinterRose.WinterThornScripting.Interpreting;

namespace WinterRose.Monogame.Thorn
{
    /// <summary>
    /// A component that allows for scripting in WinterThorn
    /// </summary>
    public class ThornScript : ObjectBehavior
    {
        /// <summary>
        /// The script that this component is taking its class from
        /// </summary>
        public WinterThorn Script { get; }
        /// <summary>
        /// The class that this component is taking its functions from
        /// </summary>
        public Class operatingClass { get; }
        /// <summary>
        /// The Awake function of the class. May be null if the class does not have a Awake function
        /// </summary>
        public Function? AwakeFunc { get; }
        /// <summary>
        /// The Start function of the class. May be null if the class does not have a Start function
        /// </summary>
        public Function? StartFunc { get; }
        /// <summary>
        /// The Update function of the class. May be null if the class does not have a Update function
        /// </summary>
        public Function? UpdateFunc { get; }
        /// <summary>
        /// The Close function of the class. May be null if the class does not have a Close function
        /// </summary>
        public Function? CloseFunc { get; }

        public ThornScript()
        {
        }
        public ThornScript(WinterThorn script, Class @class)
        {
            Script = script;
            AwakeFunc = @class.Block.Functions.FirstOrDefault(x => x.Name == "Awake");
            StartFunc = @class.Block.Functions.FirstOrDefault(x => x.Name == "Start");
            UpdateFunc = @class.Block.Functions.FirstOrDefault(x => x.Name == "Update");
            CloseFunc = @class.Block.Functions.FirstOrDefault(x => x.Name == "Close");
            operatingClass = @class;

            @class.DeclareVariable(new Variable("transform", "the components transform", AccessControl.Public)
            {
                Value = () =>
                {
                    return new ThornTransform(transform).GetClass();
                },
                Setter = (Variable var) => { }
            });
        }

        public ThornScript(string scriptPath)
        {
            string code = FileManager.Read(scriptPath);
            string name = Path.GetFileNameWithoutExtension(scriptPath);

            Script = new WinterThorn(code, name, "IngameScript", "TheGame", new(1, 0, 0));
            Script.DefineNamespace(MonoUtils.CreateWinterThornNamespace());

            Class? @class = (Script.Namespaces[1]?.Classes.FirstOrDefault())
                ?? throw new InvalidOperationException($"Thorn script {scriptPath} does not contain a class.");

            AwakeFunc = @class.Block.Functions.FirstOrDefault(x => x.Name == "Awake");
            StartFunc = @class.Block.Functions.FirstOrDefault(x => x.Name == "Start");
            UpdateFunc = @class.Block.Functions.FirstOrDefault(x => x.Name == "Update");
            CloseFunc = @class.Block.Functions.FirstOrDefault(x => x.Name == "Close");
            operatingClass = @class;

            @class.DeclareVariable(new Variable("transform", "the components transform", AccessControl.Public)
            {
                Value = () =>
                {
                    return new ThornTransform(transform).GetClass();
                },
                Setter = (Variable var) => { }
            });
        }

        protected override void Update()
        {
            UpdateFunc?.Invoke();

            object o = operatingClass.Block["transform"].Value;
            Type t = o.GetType();
            if(o is not Class s || s.CSharpClass is not ThornTransform)
                throw new WinterThornExecutionError(ThornError.InvalidType, "WTM-0001", "The 'transform' variable should not be set to something else");
        }

        protected override void Start()
        {
            StartFunc?.Invoke();
        }

        protected override void Awake()
        {
            AwakeFunc?.Invoke();
        }

        protected override void Close()
        {
            CloseFunc?.Invoke();
        }
    }
}
