using System;
using System.Collections.Generic;
using System.Linq;

namespace goal {
    class Program {
        public static Random rnd = new Random (DateTime.Now.Millisecond);
        static void Main (string[] args) {
            Func<Cell, bool> judge = (neighbour) => {
                return neighbour.getHistory ().Last ();
            };
            var grid = new Grid (10, judge);

            // // Production
            // int command = 0;
            // int iteration = 0;
            // Console.WriteLine (string.Format ("~~~ Gen {0} ~~~", 0));
            // grid.Print ();
            // Console.Write ("> ");
            // command = Console.Read ();
            // while (command == 10 && command != 113) {
            //     //Console.Clear ();
            //     Console.ResetColor ();
            //     iteration += 1;
            //     Console.WriteLine (string.Format ("~~~ Gen {0} ~~~", iteration));
            //     grid.step ();
            //     grid.Print ();
            //     Console.Write ("> ");
            //     command = Console.Read ();
            // }

            // Debug
            for (int i = 0; i < 100; i++) {
                //grid.Print ();
                grid.step ();
                //Console.WriteLine (string.Format ("~~~ Gen {0} ~~~", i));
                //
            }
            grid.Print ();
        }
    }

    public class Grid : Dictionary<Point, Cell> {

        public Grid (int dimention, Func<Cell, bool> judge) {
            this.judge = judge;
            var random = new Random (DateTime.Now.Millisecond);
            for (int i = 0; i < dimention * dimention; i++) {
                this.Add (new Point (i / dimention, i % dimention),
                    new Cell (history => {
                        int longestStreak = 0;
                        int currentStreak = 0;

                        foreach (var state in history) {
                            if (state) {
                                currentStreak++;
                                if (currentStreak > longestStreak)
                                    longestStreak = currentStreak;

                                continue;
                            }

                            currentStreak = 0;
                        }

                        return longestStreak;
                    }));
            }
        }
        private Func<Cell, bool> judge = (neighbour) => {
            return neighbour.getHistory ().Last ();
        };

        private Func<IEnumerable<Cell>, bool> filter = (neighbours) => {
            return neighbours.Count () == 1;
        };

        public void step () {
            foreach (var item in this) {
                var address = item.Key;
                var cell = item.Value;
                var neighbourhood = this.getNeighbourhood (address);
                var newState = filter (neighbourhood.Where (neighbour => judge (neighbour)));
                cell.commit (newState);
            }
            foreach (var item in this) {
                var cell = item.Value;
                cell.commit ();
            }
        }
    }

    public class Cell {
        private List<bool> history;
        private bool newState;
        private Func<IEnumerable<bool>, decimal> weight;

        public Cell () {
            history = new List<bool> ();
            history.Add (Generator.RandomState ());
        }

        public Cell (Func<IEnumerable<bool>, decimal> weight) : this () {
            this.weight = weight;
        }

        public IEnumerable<bool> getHistory () {
            return history;
        }
        public void commit (bool state) {
            newState = state;
        }
        public void stage (bool state) {
            newState = state;
        }
        public void commit () {
            history.Add (newState);
        }
        public decimal getWeight () {
            if (weight != null) {
                return weight (history);
            } else {
                return history.Count (state => state == true);
            }
        }
    }
    public class Point {
        public Point (int row, int column) {
            this.Row = row;
            this.Column = column;
        }

        public int Row { get; private set; }
        public int Column { get; private set; }

        public override bool Equals (object obj) {
            if (obj == null || GetType () != obj.GetType ()) {
                return false;
            }

            var other = (Point) obj;
            return this.Row == other.Row && this.Column == other.Column;
        }

        public override int GetHashCode () {
            return string.Format ("{0}{1}", this.Row, this.Column).GetHashCode ();
        }

        public override string ToString () {
            return string.Format ("[{0},{1}]", this.Row, this.Column);
        }
    }
    public static class GridExtensions {
        public static List<Cell> getNeighbourhood (this Grid grid, Point reference_point) {
            var neighbourhood = new List<Cell> ();

            Cell left = null;
            if (grid.TryGetValue (new Point (reference_point.Row, reference_point.Column - 1), out left))
                neighbourhood.Add (left);

            Cell top_left = null;
            if (grid.TryGetValue (new Point (reference_point.Row - 1, reference_point.Column - 1), out top_left))
                neighbourhood.Add (top_left);

            Cell top = null;
            if (grid.TryGetValue (new Point (reference_point.Row - 1, reference_point.Column), out top))
                neighbourhood.Add (top);

            Cell top_right = null;
            if (grid.TryGetValue (new Point (reference_point.Row - 1, reference_point.Column + 1), out top_right))
                neighbourhood.Add (top_right);

            Cell right = null;
            if (grid.TryGetValue (new Point (reference_point.Row, reference_point.Column + 1), out right))
                neighbourhood.Add (right);

            Cell bottom_right = null;
            if (grid.TryGetValue (new Point (reference_point.Row + 1, reference_point.Column + 1), out bottom_right))
                neighbourhood.Add (bottom_right);

            Cell bottom = null;
            if (grid.TryGetValue (new Point (reference_point.Row + 1, reference_point.Column), out bottom))
                neighbourhood.Add (bottom);

            Cell bottom_left = null;
            if (grid.TryGetValue (new Point (reference_point.Row + 1, reference_point.Column - 1), out bottom_left))
                neighbourhood.Add (bottom_left);

            return neighbourhood;
        }
    }
    public static class Generator {
        static Random random = new Random (DateTime.Now.Millisecond);
        public static bool RandomState () {
            return random.Next (0, 2) == 1;
        }
    }
    public static class Printer {
        public static void Print (this Grid grid) {
            foreach (var item in grid) {
                var location = item.Key;
                var cell = item.Value;

                var weight = cell.getWeight ();
                var print_line = string.Format ("[{0}]\t", weight);
                //Console.ForegroundColor = cell.getHistory ().Last () == 1 ? ConsoleColor.Green : ConsoleColor.Red;

                Console.Write (print_line);

                if (location.Column == Math.Sqrt (grid.Count) - 1) {
                    Console.WriteLine ();
                    Console.WriteLine ();
                }
            }
            Console.ResetColor ();
        }
    }
}