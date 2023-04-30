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

using SharpMp4Parser.Tools;
using System.IO;
using System.Text;
using System;
using System.Diagnostics;

namespace SharpMp4Parser
{
    /**
 * A Property file based BoxFactory
 */
    public class PropertyBoxParserImpl : AbstractBoxParser
    {
        public static Properties BOX_MAP_CACHE = null;
        public Properties mapping;

        static string[] EMPTY_STRING_ARRAY = new string[0];
        Pattern constuctorPattern = Pattern.compile("(.*)\\((.*?)\\)");
        StringBuilder buildLookupStrings = new StringBuilder();
        ThreadLocal<string> clazzName = new ThreadLocal<>();
        ThreadLocal<string[]> param = new ThreadLocal<>();

        public PropertyBoxParserImpl(params string[] customProperties)
        {
            if (BOX_MAP_CACHE != null)
            {
                mapping = new Properties(BOX_MAP_CACHE);
            }
            else
            {
                InputStream ins = ClassLoader.getSystemResourceAsStream("isoparser2-default.properties");
                try
                {
                    mapping = new Properties();
                    try
                    {
                        mapping.load(ins);
                        ClassLoader cl = Thread.currentThread().getContextClassLoader();
                        if (cl == null)
                        {
                            cl = ClassLoader.getSystemClassLoader();
                        }
                        Enumeration<URL> enumeration = cl.getResources("isoparser-custom.properties");

                        while (enumeration.hasMoreElements())
                        {
                            URL url = enumeration.nextElement();
                            using (InputStream customIS = url.openStream())
                            {
                                mapping.load(customIS);
                            }
                        }
                    for (string customProperty in customProperties)
                        {
                            mapping.load(getClass().getResourceAsStream(customProperty));
                        }
                        BOX_MAP_CACHE = mapping;
                    } catch (IOException e)
                    {
                        throw;
                    }
                }
                finally
                {
                    try
                    {
                        if (ins != null)
                        {
                            ins.close();
                        }
                    }
                    catch (IOException e)
                    {
                        Debug.WriteLine(e.StackTrace);
                        // ignore - I can't help
                    }
                }
            }
        }

        public PropertyBoxParserImpl(Properties mapping)
        {
            this.mapping = mapping;
        }

        public override ParsableBox createBox(string type, byte[] userType, string parent)
        {

            invoke(type, userType, parent);
            string[] param = this.param.get();
            try
            {
                Type clazz = (Type)Type.GetType(clazzName.get());
                if (param.Length > 0)
                {
                    Type[] constructorArgsClazz = new Type[param.Length];
                    object[] constructorArgs = new object[param.Length];
                    for (int i = 0; i < param.Length; i++)
                    {
                        if ("userType".Equals(param[i]))
                        {
                            constructorArgs[i] = userType;
                            constructorArgsClazz[i] = typeof(byte[]);
                        } else if ("type".Equals(param[i]))
                        {
                            constructorArgs[i] = type;
                            constructorArgsClazz[i] = typeof(string);
                        } else if ("parent".Equals(param[i]))
                        {
                            constructorArgs[i] = parent;
                            constructorArgsClazz[i] = typeof(string);
                        } else
                        {
                            throw new Exception("No such param: " + param[i]);
                        }
                    }

                    Constructor<ParsableBox> constructorObject = clazz.getConstructor(constructorArgsClazz);
                    return constructorObject.newInstance(constructorArgs);
                } else
                {
                    return clazz.getDeclaredConstructor().newInstance();
                }

            } catch (Exception) {
                throw;
            }
        }

        public void invoke(string type, byte[] userType, string parent)
        {
            string constructor;
            if (userType != null)
            {
                if (!"uuid".Equals((type)))
                {
                    throw new Exception("we have a userType but no uuid box type. Something's wrong");
                }
                constructor = mapping.getProperty("uuid[" + Hex.encodeHex(userType).ToUpperInvariant() + "]");
                if (constructor == null)
                {
                    constructor = mapping.getProperty((parent) + "-uuid[" + Hex.encodeHex(userType).ToUpperInvariant() + "]");
                }
                if (constructor == null)
                {
                    constructor = mapping.getProperty("uuid");
                }
            }
            else
            {
                constructor = mapping.getProperty((type));
                if (constructor == null)
                {
                    String lookup = buildLookupStrings.Append(parent).Append('-').Append(type).ToString();
                    buildLookupStrings.setLength(0);
                    constructor = mapping.getProperty(lookup);

                }
            }
            if (constructor == null)
            {
                constructor = mapping.getProperty("default");
            }
            if (constructor == null)
            {
                throw new Exception("No box object found for " + type);
            }
            if (!constructor.EndsWith(")"))
            {
                param.set(EMPTY_STRING_ARRAY);
                clazzName.set(constructor);
            }
            else
            {
                Matcher m = constuctorPattern.matcher(constructor);
                bool matches = m.matches();
                if (!matches)
                {
                    throw new Exception("Cannot work with that constructor: " + constructor);
                }
                clazzName.set(m.group(1));
                if (m.group(2).length() == 0)
                {
                    param.set(EMPTY_STRING_ARRAY);
                }
                else
                {
                    param.set(m.group(2).length() > 0 ? m.group(2).split(",") : new string[] { });
                }
            }
        }
    }
}
