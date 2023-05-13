/*
 * Copyright 2012 Sebastian Annies, Hamburg
 *
 * Licensed under the Apache License, Version 2.0 (the License);
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an AS IS BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using SharpMp4Parser.IsoParser.Support;

namespace SharpMp4Parser.IsoParser.Tools
{
    public class Path
    {
        public static Regex component = new Regex("(....|\\.\\.)(\\[(.*)\\])?");

        private Path()
        {
        }

        public static T getPath<T>(Box parsableBox, string path)
        {
            List<T> all = getPaths<T>(parsableBox, path, true);
            return all.Count == 0 ? default : all[0];
        }

        public static T getPath<T>(Container container, string path)
        {
            List<T> all = getPaths<T>(container, path, true);
            return all.Count == 0 ? default : all[0];
        }

        public static T getPath<T>(AbstractContainerBox containerBox, string path)
        {
            List<T> all = getPaths<T>(containerBox, path, true);
            return all.Count == 0 ? default : all[0];
        }


        public static List<T> getPaths<T>(Box box, string path)
        {
            return getPaths<T>(box, path, false);
        }

        public static List<T> getPaths<T>(Container container, string path)
        {
            return getPaths<T>(container, path, false);
        }

        private static List<T> getPaths<T>(AbstractContainerBox container, string path, bool singleResult)
        {
            return getPaths<T>((object)container, path, singleResult);
        }

        private static List<T> getPaths<T>(Container container, string path, bool singleResult)
        {
            return getPaths<T>((object)container, path, singleResult);
        }

        private static List<T> getPaths<T>(ParsableBox parsableBox, string path, bool singleResult)
        {
            return getPaths<T>((object)parsableBox, path, singleResult);
        }

        private static List<T> getPaths<T>(object thing, string path, bool singleResult)
        {
            if (path.StartsWith("/"))
            {
                throw new Exception("Cannot start at / - only relative path expression into the structure are allowed");
            }

            if (path.Length == 0)
            {
                if (thing is ParsableBox p)
                {
                    return new List<T> { (T)thing };
                }
                else
                {
                    throw new Exception("Result of path expression seems to be the root container. This is not allowed!");
                }
            }
            else
            {
                string later;
                string now;
                if (path.Contains("/"))
                {
                    later = path.Substring(path.IndexOf('/') + 1);
                    now = path.Substring(0, path.IndexOf('/'));
                }
                else
                {
                    now = path;
                    later = "";
                }

                Match m = component.Match(now);
                if (m.Success)
                {
                    string type = m.Groups[1].Value;
                    if ("..".Equals(type))
                    {
                        throw new Exception(".. notation no longer allowed");
                    }
                    else
                    {
                        if (thing is Container)
                        {
                            int index = -1;
                            if (m.Groups.Count >= 3 && !string.IsNullOrEmpty(m.Groups[2].Value))
                            {
                                // we have a specific index
                                string indexString = m.Groups[3].Value;
                                index = int.Parse(indexString);
                            }
                            List<T> children = new List<T>();
                            int currentIndex = 0;
                            // I'm suspecting some Dalvik VM to create indexed loops from for-each loops
                            // using the iterator instead makes sure that this doesn't happen
                            // (and yes - it could be completely useless)
                            List<Box>.Enumerator iterator = ((Container)thing).getBoxes().GetEnumerator();
                            while (iterator.MoveNext())
                            {
                                Box box1 = iterator.Current;
                                if (box1.getType().CompareTo(type) == 0)
                                {
                                    if (index == -1 || index == currentIndex)
                                    {
                                        children.AddRange(getPaths<T>(box1, later, singleResult));
                                    }
                                    currentIndex++;
                                }
                                if ((singleResult || index >= 0) && children.Count != 0)
                                {
                                    return children;
                                }
                            }
                            return children;
                        }
                        else
                        {
                            return new List<T>();
                        }
                    }
                }
                else
                {
                    throw new Exception(now + " is invalid path.");
                }
            }
        }

        public static bool isContained(Container refc, Box box, string path)
        {
            return getPaths<Box>(refc, path).Contains(box);
        }
    }
}
