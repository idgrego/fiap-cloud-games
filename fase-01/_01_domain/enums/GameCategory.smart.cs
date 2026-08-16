namespace fase_01.domain.enums
{
    public class GameCategory
    {
        public readonly byte Code;
        public readonly string Name;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameCategory"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="name">The name.</param>
        private GameCategory(byte code, string name)
        {
            this.Code = code;
            this.Name = name;
        }

        public static readonly GameCategory Unknown = new(0, "Unknown");
        public static readonly GameCategory Action = new(1, "Action");
        public static readonly GameCategory Adventure = new(2, "Adventure");
        public static readonly GameCategory RolePlaying = new(3, "RolePlaying");
        public static readonly GameCategory Simulation = new(4, "Simulation");
        public static readonly GameCategory Strategy = new(5, "Strategy");
        public static readonly GameCategory Sports = new(6, "Sports");
        public static readonly GameCategory Puzzle = new(7, "Puzzle");
        public static readonly GameCategory Racing = new(8, "Racing");
        public static readonly GameCategory Fighting = new(9, "Fighting");
        public static readonly GameCategory Horror = new(10, "Horror");

        public static IEnumerable<GameCategory> List()
        {
            return [Unknown, Action, Adventure, RolePlaying, Simulation, Strategy, Sports, Puzzle, Racing, Fighting, Horror];
        }

        public static GameCategory FromCode(byte code)
        {
            return code switch
            {
                1 => Action,
                2 => Adventure,
                3 => RolePlaying,
                4 => Simulation,
                5 => Strategy,
                6 => Sports,
                7 => Puzzle,
                8 => Racing,
                9 => Fighting,
                10 => Horror,
                _ => Unknown
            };
        }

        public static GameCategory FromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Unknown;
            name = name.Trim().ToLower();
            return name switch
            {
                "action" => Action,
                "adventure" => Adventure,
                "roleplaying" => RolePlaying,
                "simulation" => Simulation,
                "strategy" => Strategy,
                "sports" => Sports,
                "puzzle" => Puzzle,
                "racing" => Racing,
                "fighting" => Fighting,
                "horror" => Horror,
                _ => Unknown
            };
        }
    }
}