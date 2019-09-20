﻿using LaixerGMLTest.BAG_Objects;
using LaixerGMLTest.Object_Relations;
using NetTopologySuite.IO.GML2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;



namespace LaixerGMLTest
{
    /// <summary>
    /// A custom reader for BAG XML Files
    /// </summary>
    class LaixerBagReader
    {

        public string logText;
        public string xmlOutput;

        public List<BAGObject> listOfBAGObjects;
        BAGObjectFactory BAGObjectFactory = new BAGObjectFactory();

        public LaixerBagReader()
        {
            listOfBAGObjects = new List<BAGObject>();
        }

        /// <summary>
        /// This can read the Whole XML file within +-35 seconds
        /// </summary>
        public void ReadXML()
        {
            WithXMLReaderAsync("nope").Wait();
        }

        /// <summary>
        /// Reads an XML file
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        public void ReadXML(string filePath)
        {
            //read the xml file Async
            WithXMLReaderAsync(filePath).Wait();
        }

        /// <summary>
        /// Reads a XML document in a async way
        /// </summary>
        /// <param name="xmlFile">Path to the XML file</param>
        /// <returns></returns>
        private async Task WithXMLReaderAsync(string xmlFile)
        {
            XmlReaderSettings settings = new XmlReaderSettings
            {
                Async = true,
                IgnoreWhitespace = true,
            };

            // used to start reading the file from top to bottom. 
            using (XmlReader reader = XmlReader.Create(xmlFile, settings))
            {
                while (await reader.ReadAsync())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            {
                                await CheckRootElement(reader).ConfigureAwait(false);
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
            }
        }

        /// <summary>
        /// Checks the roottype of the XML file
        /// </summary>
        /// <param name="reader">The xml reader</param>
        /// <returns></returns>
        private async Task CheckRootElement(XmlReader reader)
        {
            Console.WriteLine($"Root element: {reader.LocalName}");
            switch (reader.LocalName)
            {
                case "BAG-Extract-Deelbestand-LVC":
                    {
                        await ReadXMLBody(reader).ConfigureAwait(false);
                        break;
                    }
                case "BAG-Mutaties-Deelbestand-LVC":
                    {
                        break;
                    }
                case "BAG-Extract-Levering":
                    {
                        break;
                    }

                default:
                    break;
            }
        }

        private async Task ReadXMLBody(XmlReader reader)
        {
            while (await reader.ReadAsync())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        {
                            await PrefixReader(reader).ConfigureAwait(false);
                            break;
                        }

                    case XmlNodeType.Text:
                        {
                            break;
                        }

                    case XmlNodeType.EndElement:
                        {
                            break;
                        }

                    default:
                        {
                            break;
                        }
                }
            }
        }

        public async Task PrefixReader(XmlReader reader)
        {
            switch (reader.Prefix)
            {
                case "xb - remove this to use it -":
                    {
                        break;
                    }
                case "selecties-extract":
                    {
                        break;
                    }

                case "bag_LVC":
                    {
                        await BAGObjectGenerator(reader).ConfigureAwait(false);
                        break;
                    }
                default:
                    break;
            }
        }


        public async Task BAGObjectGenerator(XmlReader reader)
        {
            switch (reader.LocalName)
            {
                case "Ligplaats":
                    {
                        var elementName = "";
                        var nameOfelement = reader.LocalName;
                        var myObject = (Berth)BAGObjectFactory.GetBagObjectByXML(nameOfelement);
                        listOfBAGObjects.Add(myObject);

                        // fill the object with al the stuff that we can find in the xml file
                        while (reader.Read())
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    {
                                        elementName = reader.LocalName;
                                        if (reader.LocalName.ToLower() == "polygon")
                                        {
                                            var value = await reader.ReadOuterXmlAsync();
                                            // Store the value in the property geovlak of this object
                                            myObject.SetAttribute("geovlak", value);
                                        }
                                        // Transform the date-time string to a DateTime object when these two names are found
                                        if (reader.LocalName.ToLower() == "begindatumtijdvakgeldigheid" || reader.LocalName.ToLower() == "einddatumtijdvakgeldigheid")
                                        {
                                            // Go to next part
                                            reader.Read();
                                            // Read the value and transform it into a DateTime object
                                            var r = normalizeDateTime(await reader.GetValueAsync());
                                            // Set the attribute
                                            myObject.SetAttribute(elementName, r);
                                        }
                                        // Transform the date string to a DateTime object
                                        if (reader.LocalName.ToLower() == "documentdatum")
                                        {
                                            // Go to next part
                                            reader.Read();
                                            // Get the date string
                                            var r = normalizeDate(await reader.GetValueAsync());
                                            // Set the attribute
                                            myObject.SetAttribute(elementName, r);
                                        }
                                        if (reader.LocalName.ToLower() == "hoofdadres")
                                        {
                                            //skip one node to read the text
                                            reader.Read();
                                        }
                                        break;
                                    }
                                case XmlNodeType.Text:
                                    {
                                        // retrieve the value in the node
                                        string value = await reader.GetValueAsync().ConfigureAwait(false);
                                        myObject.SetAttribute(elementName, value);
                                        break;
                                    }
                                case XmlNodeType.EndElement:
                                    {
                                        // If the end element is reached
                                        if (reader.LocalName == nameOfelement)
                                        {
                                            // We can get out of this function, because we reached the end tag of this element
                                            return;
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                case "Woonplaats":
                    {
                        var elementName = "";
                        var nameOfelement = reader.LocalName;
                        var myObject = (Residence)BAGObjectFactory.GetBagObjectByXML(nameOfelement);
                        listOfBAGObjects.Add(myObject);

                        // Fill the object with al the stuff that we can find in the xml file
                        while (reader.Read())
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    {
                                        // Store the name of the current element
                                        elementName = reader.LocalName;
                                        if (reader.LocalName.ToLower() == "polygon")
                                        {
                                            var value = await reader.ReadOuterXmlAsync();
                                            // Store the value in the property geovlak of this object
                                            myObject.SetAttribute("geovlak", value);
                                        }
                                        // Transform the date-time string to a DateTime object when these two names are found
                                        if (reader.LocalName.ToLower() == "begindatumtijdvakgeldigheid" || reader.LocalName.ToLower() == "einddatumtijdvakgeldigheid")
                                        {
                                            // Go to next part
                                            reader.Read();
                                            // Read the value and transform it into a DateTime object
                                            var r = normalizeDateTime(await reader.GetValueAsync());
                                            // Set the attribute
                                            myObject.SetAttribute(elementName, r);
                                        }
                                        // Transform the date string to a DateTime object
                                        if(reader.LocalName.ToLower() == "documentdatum")
                                        {
                                            // Go to next part
                                            reader.Read();
                                            // Get the date string
                                            var r = normalizeDate(await reader.GetValueAsync());
                                            // Set the attribute
                                            myObject.SetAttribute(elementName, r);
                                        }
                                        break;
                                    }
                                case XmlNodeType.Text:
                                    {
                                        // retrieve the value in the node
                                        string value = await reader.GetValueAsync().ConfigureAwait(false);
                                        myObject.SetAttribute(elementName, value);
                                        break;
                                    }
                                case XmlNodeType.EndElement:
                                    {
                                        // write the end element name. For testing purpouse
                                        if (reader.LocalName == nameOfelement)
                                        {
                                            // We can get out of this function, because we reached the end tag of this element
                                            return;
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                case "Verblijfsobject":
                    {
                        var elementName = "";
                        var nameOfelement = reader.LocalName;
                        var myObject = (Accommodation)BAGObjectFactory.GetBagObjectByXML(nameOfelement);
                        listOfBAGObjects.Add(myObject);

                        // fill the object with al the stuff that we can find in the xml file
                        while (reader.Read())
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    {
                                        elementName = reader.LocalName;
                                        if (reader.LocalName == "gerelateerdPand" || reader.LocalName == "hoofdadres")
                                        {
                                            // skip once to get to the id
                                            reader.Read();
                                        }

                                        if (reader.LocalName.ToLower() == "point")
                                        {
                                            string value = await reader.ReadOuterXmlAsync();
                                            // Store the value in the property geovlak and point of this object
                                            var value2 = value.Replace("<gml:pos>", "<gml:pos srsDimension=\"3\">");
                                            myObject.SetAttribute("geopunt", value2);
                                        }
                                        if (reader.LocalName.ToLower() == "polygon")
                                        {
                                            string value = await reader.ReadOuterXmlAsync();
                                            // Store the value in the property geovlak of this object
                                            myObject.SetAttribute("geovlak", value);
                                        }

                                        // Transform the date-time string to a DateTime object when these two names are found
                                        if (reader.LocalName.ToLower() == "begindatumtijdvakgeldigheid" || reader.LocalName.ToLower() == "einddatumtijdvakgeldigheid")
                                        {
                                            // Go to next part
                                            reader.Read();
                                            // Read the value and transform it into a DateTime object
                                            var r = normalizeDateTime(await reader.GetValueAsync());
                                            // Set the attribute
                                            myObject.SetAttribute(elementName, r);
                                        }

                                        // Transform the date string to a DateTime object
                                        if (reader.LocalName.ToLower() == "documentdatum")
                                        {
                                            // Go to next part
                                            reader.Read();
                                            // Get the date string
                                            var r = normalizeDate(await reader.GetValueAsync());
                                            // Set the attribute
                                            myObject.SetAttribute(elementName, r);
                                        }
                                        break;
                                    }
                                case XmlNodeType.Text:
                                    {
                                        // Retrieve the value in the node
                                        string value = await reader.GetValueAsync().ConfigureAwait(false);

                                        myObject.SetAttribute(elementName, value);
                                        break;
                                    }
                                case XmlNodeType.EndElement:
                                    {
                                        // Write the end element name. For testing purpouse
                                        if (reader.LocalName == nameOfelement)
                                        {
                                            // We can get out of this function, because we reached the end tag of this element
                                            return;
                                        }

                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                case "OpenbareRuimte":
                    {
                        var elementName = "";
                        var nameOfelement = reader.LocalName;
                        var myObject = (PublicSpace)BAGObjectFactory.GetBagObjectByXML(nameOfelement);
                        listOfBAGObjects.Add(myObject);

                        // fill the object with al the stuff that we can find in the xml file
                        while (reader.Read())
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    {
                                        elementName = reader.LocalName;
                                        if (reader.LocalName.ToLower() == "gerelateerdewoonplaats")
                                        {
                                            // read next node
                                            reader.Read();
                                        }

                                        // Transform the date-time string to a DateTime object when these two names are found
                                        if (reader.LocalName.ToLower() == "begindatumtijdvakgeldigheid" || reader.LocalName.ToLower() == "einddatumtijdvakgeldigheid")
                                        {
                                            // Go to next part
                                            reader.Read();
                                            // Read the value and transform it into a DateTime object
                                            var r = normalizeDateTime(await reader.GetValueAsync());
                                            // Set the attribute
                                            myObject.SetAttribute(elementName, r);
                                        }

                                        // Transform the date string to a DateTime object
                                        if (reader.LocalName.ToLower() == "documentdatum")
                                        {
                                            // Go to next part
                                            reader.Read();
                                            // Get the date string
                                            var r = normalizeDate(await reader.GetValueAsync());
                                            // Set the attribute
                                            myObject.SetAttribute(elementName, r);
                                        }

                                        break;
                                    }
                                case XmlNodeType.Text:
                                    {
                                        // retrieve the value in the node
                                        string value = await reader.GetValueAsync().ConfigureAwait(false);

                                        myObject.SetAttribute(elementName, value);
                                        break;
                                    }
                                case XmlNodeType.EndElement:
                                    {
                                        // write the end element name. For testing purpouse
                                        if (reader.LocalName == nameOfelement)
                                        {
                                            // We can get out of this function, because we reached the end tag of this element
                                            return;
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                case "Nummeraanduiding":
                    {
                        var elementName = "";
                        var nameOfelement = reader.LocalName;
                        var myObject = (NumberIndication)BAGObjectFactory.GetBagObjectByXML(nameOfelement);
                        listOfBAGObjects.Add(myObject);

                        // fill the object with al the stuff that we can find in the xml file
                        while (reader.Read())
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    {
                                        elementName = reader.LocalName;
                                        // Transform the date-time string to a DateTime object when these two names are found
                                        if (reader.LocalName.ToLower() == "begindatumtijdvakgeldigheid" || reader.LocalName.ToLower() == "einddatumtijdvakgeldigheid")
                                        {
                                            // Go to next part
                                            reader.Read();
                                            // Read the value and transform it into a DateTime object
                                            var r = normalizeDateTime(await reader.GetValueAsync());
                                            // Set the attribute
                                            myObject.SetAttribute(elementName, r);
                                        }

                                        // Transform the date string to a DateTime object
                                        if (reader.LocalName.ToLower() == "documentdatum")
                                        {
                                            // Go to next part
                                            reader.Read();
                                            // Get the date string
                                            var r = normalizeDate(await reader.GetValueAsync());
                                            // Set the attribute
                                            myObject.SetAttribute(elementName, r);
                                        }
                                        if(reader.LocalName == "gerelateerdeOpenbareRuimte" || reader.LocalName == "gerelateerdeWoonplaats")
                                        {
                                            // Go to next part of the element
                                            reader.Read();

                                            var value = await reader.GetValueAsync();
                                            myObject.SetAttribute(elementName, value);
                                        }


                                        break;
                                    }
                                case XmlNodeType.Text:
                                    {
                                        // retrieve the value in the node
                                        string value = await reader.GetValueAsync().ConfigureAwait(false);
                                        myObject.SetAttribute(elementName, value);
                                        break;
                                    }
                                case XmlNodeType.EndElement:
                                    {
                                        // write the end element name. For testing purpouse
                                        if (reader.LocalName == nameOfelement)
                                        {
                                            // We can get out of this function, because we reached the end tag of this element
                                            //myObject.ShowAllAttributes();
                                            return;
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                case "Standplaats":
                    {
                        var elementName = "";
                        var nameOfelement = reader.LocalName;
                        var myObject = (Location)BAGObjectFactory.GetBagObjectByXML(nameOfelement);
                        listOfBAGObjects.Add(myObject);

                        // fill the object with al the stuff that we can find in the xml file
                        while (reader.Read())
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    {
                                        elementName = reader.LocalName;
                                        break;
                                    }
                                case XmlNodeType.Text:
                                    {
                                        // retrieve the value in the node
                                        string value = await reader.GetValueAsync().ConfigureAwait(false);
                                        myObject.SetAttribute(elementName, value);
                                        break;
                                    }
                                case XmlNodeType.EndElement:
                                    {
                                        // write the end element name. For testing purpouse
                                        if (reader.LocalName == nameOfelement)
                                        {
                                            // We can get out of this function, because we reached the end tag of this element
                                            return;
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                case "Pand":
                    {
                        // store the name of the element to read.
                        var elementName = "";
                        // Store the name of the Object
                        var nameOfelement = reader.LocalName;
                        // Create a new bag object based on the name of the object
                        var myObject = (Premises)BAGObjectFactory.GetBagObjectByXML(nameOfelement);
                        // Add the object to the list 
                        listOfBAGObjects.Add(myObject);

                        // Fill the object with al the stuff that we can find in the xml file
                        while (reader.Read())
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    {
                                        elementName = reader.LocalName;
                                        if (reader.LocalName.ToLower() == "polygon")
                                        {
                                            // Insert the list of position data into the attribute :geovlak
                                            var value = await reader.ReadOuterXmlAsync();
                                            myObject.SetAttribute("geovlak", value);
                                        }
                                        // Transform the date-time string to a DateTime object when these two names are found
                                        if (reader.LocalName.ToLower() == "begindatumtijdvakgeldigheid" || reader.LocalName.ToLower() == "einddatumtijdvakgeldigheid")
                                        {
                                            // Go to next part
                                            reader.Read();
                                            // Read the value and transform it into a DateTime object
                                            var r = normalizeDateTime(await reader.GetValueAsync());
                                            // Set the attribute
                                            myObject.SetAttribute(elementName, r);
                                        }
                                        // Transform the date string to a DateTime object
                                        if (reader.LocalName.ToLower() == "documentdatum")
                                        {
                                            // Go to next part
                                            reader.Read();
                                            // Get the date string
                                            var r = normalizeDate(await reader.GetValueAsync());
                                            // Set the attribute
                                            myObject.SetAttribute(elementName, r);
                                        }
                                        break;
                                    }
                                case XmlNodeType.Text:
                                    {
                                        // Retrieve the value in the node
                                        string value = await reader.GetValueAsync().ConfigureAwait(false);

                                        myObject.SetAttribute(elementName, value);
                                        break;
                                    }
                                case XmlNodeType.EndElement:
                                    {
                                        // Write the end element name. For testing purpouse
                                        if (reader.LocalName == nameOfelement)
                                        {
                                            // We can get out of this function, because we reached the end tag of this element
                                            return;
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                case "GemeenteWoonplaatsRelatie":
                    {
                        var elementName = "";
                        var nameOfelement = reader.LocalName;
                        var myObject = (MunicipalityResidenceRelation)BAGObjectFactory.GetBagObjectByXML(nameOfelement);
                        listOfBAGObjects.Add(myObject);

                        // fill the object with al the stuff that we can find in the xml file
                        while (reader.Read())
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    {
                                        elementName = reader.LocalName;

                                        break;
                                    }
                                case XmlNodeType.Text:
                                    {
                                        // retrieve the value in the node
                                        string value = await reader.GetValueAsync().ConfigureAwait(false);

                                        myObject.SetAttribute(elementName, value);
                                        break;
                                    }
                                case XmlNodeType.EndElement:
                                    {
                                        // write the end element name. For testing purpouse
                                        if (reader.LocalName == nameOfelement)
                                        {
                                            // We can get out of this function, because we reached the end tag of this element
                                            return;
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                default:
                    break;
            }
        }

        private string ReadGMLAttributes(XmlReader reader)
        {
            var myReader = reader;
            var gmlReader = new GMLReader(new NetTopologySuite.Geometries.GeometryFactory(new NetTopologySuite.Geometries.PrecisionModel(), 0));
            var result = gmlReader.Read(myReader);
            var temp = result.Coordinates.ToList();

            string gmlString = "";
            foreach (var item in temp)
            {
                if (gmlString == "") { gmlString = $"{item.CoordinateValue}"; }

                gmlString = $"{gmlString},{item.CoordinateValue}";
            }
            return gmlString;
        }

        private void printAllAttributes()
        {
            if (listOfBAGObjects[0].GetType() == typeof(Berth))
            {
                var item = (Berth)listOfBAGObjects[0];
                item.ShowAllAttributes();
            }
            else if (listOfBAGObjects[0].GetType() == typeof(Location))
            {
                var item = (Location)listOfBAGObjects[0];
                item.ShowAllAttributes();
            }
            else if (listOfBAGObjects[0].GetType() == typeof(Premises))
            {
                var item = (Premises)listOfBAGObjects[0];
                item.ShowAllAttributes();
            }
            else if (listOfBAGObjects[0].GetType() == typeof(NumberIndication))
            {
                var item = (NumberIndication)listOfBAGObjects[0];
                item.ShowAllAttributes();
            }
            else if (listOfBAGObjects[0].GetType() == typeof(Residence))
            {
                var item = (Residence)listOfBAGObjects[0];
                item.ShowAllAttributes();
            }
            else if (listOfBAGObjects[0].GetType() == typeof(PublicSpace))
            {
                var item = (PublicSpace)listOfBAGObjects[0];
                item.ShowAllAttributes();
            }
            else if (listOfBAGObjects[0].GetType() == typeof(Accommodation))
            {
                var item = (Accommodation)listOfBAGObjects[0];
                item.ShowAllAttributes();
            }

        }

        private DateTime normalizeDateTime(string time)
        {
            // Split and store the date and time separate
            var year = int.Parse(time.Substring(0, 4));
            var month = int.Parse(time.Substring(4, 2));
            var day = int.Parse(time.Substring(6, 2));
            var Hour = int.Parse(time.Substring(8, 2));
            var minute = int.Parse(time.Substring(10, 2));
            var seconds = int.Parse(time.Substring(12, 2));
            var microseconds = int.Parse(time.Substring(14, 2));

            // Create a new DateTime variable with the variables from above
            return new DateTime(year: year, month: month, day: day, hour: Hour, minute: minute, second: seconds, millisecond: microseconds);
        }

        /// <summary>
        /// normalize the datetime string ( ISO8106)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private DateTime normalizeDate(string time)
        {
            // Split and store
            var year = int.Parse(time.Substring(0, 4));
            var month = int.Parse(time.Substring(4, 2));
            var day = int.Parse(time.Substring(6, 2));

            // Create a new DateTime object
            return new DateTime(year: year, month: month, day: day);
        }

    }
}
