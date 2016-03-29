﻿using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace ContentCompiler
{
    class Program
    {
        class Arguments
        {
            public bool Decompile { get; set; }
            [PowerCommandParser.Position(0)]
            public string ContentRoot { get; set; } = @"C:\Program Files(x86)\steam\steamapps\common\Stardew Valley\Content";
        }
        static void Main(string[] rawArgs)
        {
            var args = PowerCommandParser.Parser.ParseArguments<Arguments>(rawArgs);
            if (args.Decompile)
            {
                Decompile(args.ContentRoot);
            }
        }
        static ContentManager SetupContentManager(string root)
        {

            var serviceContainer = new Microsoft.Xna.Framework.GameServiceContainer();
            var content = new ContentManager(serviceContainer, root);
            var graphicsDeviceManager = new Microsoft.Xna.Framework.GraphicsDeviceManager(new Game1());
            serviceContainer.AddService<IGraphicsDeviceService>(graphicsDeviceManager);
            graphicsDeviceManager.CreateDevice();
            serviceContainer.AddService(typeof(GraphicsDevice), graphicsDeviceManager.GraphicsDevice);
            return content;
        }

        static void Decompile(string root)
        {
            using (var content = SetupContentManager(root))
            {
                foreach(var asset in GetGameAssetsIn<Dictionary<string, string>>(content, "characters\\schedules"))
                {
                    var schedule = Schedule.Decompile(asset);

                    File.WriteAllText(Path.Combine(root, "characters\\schedules", asset.Filename) + ".json", JsonConvert.SerializeObject(schedule, Formatting.Indented));
                }
                DecompilePortraits(content);
            }
        }
        static IEnumerable<GameAsset<T>> GetGameAssetsIn<T>(ContentManager content, string relativePath)
        {
            var items = Directory.EnumerateFiles(Path.Combine(content.RootDirectory, relativePath)).Where(c => Path.GetExtension(c) == ".xnb").Select(c => Path.GetFileNameWithoutExtension(c));
            foreach(var item in items)
            {
                yield return GameAsset.Create(item, content.Load<T>(Path.Combine(relativePath, item)));
            }
        }

        static void DecompilePortraits(ContentManager content)
        {
            foreach (var asset in GetGameAssetsIn<Texture2D>(content, "portraits"))
                using (var stream = File.Create(content.RootDirectory + "\\Portraits\\" + asset.Filename + ".png"))
                    asset.Content.SaveAsPng(stream, asset.Content.Width, asset.Content.Height);
        }
    }
    class Game1 : Microsoft.Xna.Framework.Game
    {
    }
}
