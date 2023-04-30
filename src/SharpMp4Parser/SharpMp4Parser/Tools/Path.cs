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

using SharpMp4Parser.Support;
using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace SharpMp4Parser.Tools
{
    public class Path
    {
        public static Pattern component = Pattern.compile("(....|\\.\\.)(\\[(.*)\\])?");

        private Path()
        {
        }

        public static T getPath<T>(Box parsableBox, string path)
        {
            List<T> all = getPaths(parsableBox, path, true);
            return all.Count == 0 ? default(T) : all[0];
        }

        public static T getPath<T>(Container container, string path)
        {
            List<T> all = getPaths(container, path, true);
            return all.Count == 0 ? default(T) : all[0];
        }

        public static T getPath<T>(AbstractContainerBox containerBox, string path)
        {
            List<T> all = getPaths(containerBox, path, true);
            return all.Count == 0 ? default(T) : all[0];
        }


        public static List<T> getPaths<T>(Box box, string path)
        {
            return getPaths(box, path, false);
        }

        public static List<T> getPaths<T>(Container container, string path)
        {
            return getPaths(container, path, false);
        }

        private static List<T> getPaths<T>(AbstractContainerBox container, string path, bool singleResult)
        {
            return getPaths((object)container, path, singleResult);
        }

        private static List<T> getPaths<T>(Container container, string path, bool singleResult)
        {
            return getPaths((object)container, path, singleResult);
        }

        private static List<T> getPaths<T>(ParsableBox parsableBox, string path, bool singleResult)
        {
            return getPaths((object)parsableBox, path, singleResult);
        }

        private static List<T> getPaths<T>(object thing, string path, bool singleResult)
        {
            if (path.StartsWith("/"))
            {
                throw new Exception("Cannot start at / - only relative path expression into the structure are allowed");
            }

            if (path.Length == 0)
            {
                if (thing is ParsableBox)
                {
                    return Collections.singletonList((T)thing);
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

                Matcher m = component.matcher(now);
                if (m.matches())
                {
                    String type = m.group(1);
                    if ("..".Equals(type))
                    {
                        throw new Exception(".. notation no longer allowed");
                    }
                    else
                    {
                        if (thing is Container)
                        {
                            int index = -1;
                            if (m.group(2) != null)
                            {
                                // we have a specific index
                                string indexString = m.group(3);
                                index = int.Parse(indexString);
                            }
                            List<T> children = new List<T>();
                            int currentIndex = 0;
                            // I'm suspecting some Dalvik VM to create indexed loops from for-each loops
                            // using the iterator instead makes sure that this doesn't happen
                            // (and yes - it could be completely useless)
                            Iterator<Box> iterator = ((Container)thing).getBoxes().iterator();
                            while (iterator.hasNext())
                            {
                                Box box1 = iterator.next();
                                if (box1.getType().matches(type))
                                {
                                    if (index == -1 || index == currentIndex)
                                    {
                                        children.addAll(Path.getPaths<T>(box1, later, singleResult));
                                    }
                                    currentIndex++;
                                }
                                if ((singleResult || index >= 0) && !children.Count == 0)
                                {
                                    return children;
                                }
                            }
                            return children;
                        }
                        else
                        {
                            return Collections.emptyList();
                        }
                    }
                }
                else
                {
                    throw new Exception(now + " is invalid path.");
                }
            }
        }

        public static bool isContained(Container refc, Box box, String path)
        {
            return getPaths(refc, path).contains(box);
        }
    }
}
