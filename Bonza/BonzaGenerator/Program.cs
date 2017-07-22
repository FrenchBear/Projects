// Bonza.cs
// Try to generate a bonza-like crossword
// 2017-06  PV

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using static System.Console;

namespace Bonza
{
    class BonzaApp
    {
        static void Main()
        {
            Grille g = new Grille();
            for (; !g.PlaceWords(Fruits);)
            {

            }
            g.Print();
            g.Print("C:\\temp\\fruits.txt");
            g.SaveLayout("c:\\temp\\fruits.layout");
            g.BuildPuzzle(5);

            Console.WriteLine();
            Console.Write("(Pause)");
            Console.ReadLine();
        }

        static string[] Jours = "lundi mardi mercredi jeudi vendredi samedi dimanche".Split();
        static string[] Fruits = "pêche poire pomme cerise banane kiwi vanille noix orange citron ananas mandarine clémentine mangue myrtille nèfle prune abricot noisette melon mirabelle fraise framboise pamplemousse amande cassis châtaigne coing figue datte grenade groseille litchi mûre papaye pastèque quetsche raisin brugnon griotte merise goyave".Split();
        static string[] Elements = "Hydrogen Helium Lithium Beryllium Boron Carbon Nitrogen Oxygen Fluorine Neon Sodium Magnesium Aluminium Silicon Phosphorus Sulfur Chlorine Argon Potassium Calcium Scandium Titanium Vanadium Chromium Manganese Iron Cobalt Nickel Copper Zinc Gallium Germanium Arsenic Selenium Bromine Krypton Rubidium Strontium Yttrium Zirconium Niobium Molybdenum Technetium Ruthenium Rhodium Palladium Silver Cadmium Indium Tin Antimony Tellurium Iodine Xenon Caesium Barium Lanthanum Cerium Praseodymium Neodymium Promethium Samarium Europium Gadolinium Terbium Dysprosium Holmium Erbium Thulium Ytterbium Lutetium Hafnium Tantalum Tungsten Rhenium Osmium Iridium Platinum Gold Mercury Thallium Lead Bismuth Polonium Astatine Radon Francium Radium Actinium Thorium Protactinium Uranium Neptunium Plutonium Americium Curium Berkelium Californium Einsteinium Fermium Mendelevium Nobelium Lawrencium Rutherfordium Dubnium Seaborgium Bohrium Hassium Meitnerium Darmstadtium Roentgenium Copernicium".Split();
        static string[] Zodiaque = "Bélier Taureau Gémeaux Cancer Lion Vierge Balance Scorpion Sagittaire Capricorne Verseau Poissons".Split();
        static string[] ConstellationsLatin = { "Andromeda", "Antlia Pneumatica", "Apus", "Aquarius", "Aquila", "Ara", "Aries", "Auriga", "Bootes", "Caelum", "Camelopardalis", "Cancer", "Canes Venatici", "Canis Major", "Canis Minor", "Capricornus", "Carina", "Cassiopeia", "Centaurus", "Cepheus", "Cetus", "Chamaleon", "Circinus", "Columba", "Coma Berenices", "Corona Australis", "Corona Borealis", "Corvus", "Crater", "Crux", "Cygnus", "Delphinus", "Dorado", "Draco", "Equuleus", "Eridanus", "Fornax", "Gemini", "Grus", "Hercules", "Horologium", "Hydra", "Hydrus", "Indus", "Lacerta", "Leo", "Leo Minor", "Lepus", "Libra", "Lupus", "Lynx", "Lyra", "(Mons) Mensa", "Microscopium", "Monoceros", "Musca", "Norma", "Octans", "Ophiuchus", "Orion", "Pavo", "Pegasus", "Perseus", "Phoenix", "Pictor", "Pisces", "Pisces Austrinus", "Puppis", "Pyxis", "Reticulum", "Sagitta", "Sagittarius", "Scorpius", "Sculptor", "Scutum", "Serpens", "Sextans", "Taurus", "Telescopium", "Triangulum", "Triangulum Australe", "Tucana", "Ursa Major", "Ursa Minor", "Vela", "Virgo", "Volans", "Vulpecula" };
        static string[] ConstellationsEnglish = { "Andromeda", "Air Pump", "Bird of Paradise", "Water Carrier", "Eagle", "Altar", "Ram", "Charioteer", "Herdsman", "Chisel", "Giraffe", "Crab", "Hunting Dogs", "Big Dog", "Little Dog", "Goat ( Capricorn )", "Keel", "Cassiopeia", "Centaur", "Cepheus", "Whale", "Chameleon", "Compasses", "Dove", "Berenice's Hair", "Southern Crown", "Northern Crown", "Crow", "Cup", "Southern Cross", "Swan", "Dolphin", "Goldfish", "Dragon", "Little Horse", "River", "Furnace", "Twins", "Crane", "Hercules", "Clock", "Hydra ( Sea Serpent )", "Water Serpen ( male )", "Indian", "Lizard", "Lion", "Smaller Lion", "Hare", "Balance", "Wolf", "Lynx", "Lyre", "Table", "Microscope", "Unicorn", "Fly", "Square", "Octant", "Serpent Holder", "Orion", "Peacock", "Winged Horse", "Perseus", "Phoenix", "Easel", "Fishes", "Southern Fish", "Stern", "Compass", "Reticle", "Arrow", "Archer", "Scorpion", "Sculptor", "Shield", "Serpent", "Sextant", "Bull", "Telescope", "Triangle", "Southern Triangle", "Toucan", "Great Bear", "Little Bear", "Sails", "Virgin", "Flying Fish", "Fox" };
        static string[] ConstellationsFrench = { "Andromède", "La Machine Pneumatique", "L'Oiseau du Paradis", "Le Verseau", "L'Aigle", "L'Autel", "Le Bélier", "Le Cocher", "Le Bouvier", "Le Burin (du Graveur)", "La Girafe", "Le Cancer", "Les Chiens de Chasse", "Le Grand Chien", "Le Petit Chien", "Le Capricorne", "La Carène", "Cassiopée", "Le Centaure", "Céphée", "La Baleine", "Le Caméléon", "Le Compas", "La Colombe", "La Chevelure de Bérénice", "La Couronne Australe", "La Couronne Boréale", "Le Corbeau", "La Coupe", "La Croix du Sud", "Le Cygne", "Le Dauphin", "La Dorade", "Le Dragon", "Le Petit Cheval", "L'Eridan", "Le Fourneau", "Les Gémeaux", "La Grue", "Hercule", "L'Horloge", "L'Hydre femelle", "L'Hydre mâle", "L'Oiseau Indien", "Le Lézard", "Le Lion", "Le Petit Lion", "Le Lièvre", "La Balance", "Le Loup", "Le Lynx", "La Lyre", "La Table", "Le Microscope", "La Licorne", "La Mouche", "La Règle", "L'Octant", "Le Serpentaire", "Orion", "Le Paon", "Pégase", "Persée", "Le Phénix", "Le Peintre", "Les Poissons", "Le Poisson Austal", "La Poupe", "La Boussole", "Le Réticule", "La Flèche", "Le Sagittaire", "Le Scorpion", "Le Sculpteur", "L'Ecu", "Le Serpent (Tête)", "Le Sextant", "Le Taureau", "Le Télescope", "Le Triangle", "Le Triangle Austral", "Le Toucan", "La Grande Ourse", "La Petite Ourse", "Les Voiles", "La Vierge", "Le Poisson Volant", "Le Petit Renard" };
        static string[] Countries = {
                "Afghanistan", "Albania", "Algeria", "American Samoa", "Andorra", "Angola", "Anguilla", "Antarctica", "Antigua and Barbuda", "Argentina", "Armenia", "Aruba", "Australia", "Austria", "Azerbaijan", "Bahamas", "Bahrain", "Bangladesh", "Barbados", "Belarus",
                "Belgium", "Belize", "Benin", "Bermuda", "Bhutan", "Bolivia", "Bosnia and Herzegovina", "Botswana", "Bouvet Island", "Brazil", "British Antarctic Territory", "British Indian Ocean Territory", "British Virgin Islands", "Brunei", "Bulgaria", "Burkina Faso", "Burundi", "Cambodia", "Cameroon", "Canada",
                "Canton and Enderbury Islands", "Cape Verde", "Cayman Islands", "Central African Republic", "Chad", "Chile", "China", "Christmas Island", "Cocos [Keeling] Islands", "Colombia", "Comoros", "Congo - Brazzaville", "Congo - Kinshasa", "Cook Islands", "Costa Rica", "Croatia", "Cuba", "Cyprus", "Czech Republic", "Côte d’Ivoire",
                "Denmark", "Djibouti", "Dominica", "Dominican Republic", "Dronning Maud Land", "East Germany", "Ecuador", "Egypt", "El Salvador", "Equatorial Guinea", "Eritrea", "Estonia", "Ethiopia", "Falkland Islands", "Faroe Islands", "Fiji", "Finland", "France", "French Guiana", "French Polynesia",
                "French Southern Territories", "French Southern and Antarctic Territories", "Gabon", "Gambia", "Georgia", "Germany", "Ghana", "Gibraltar", "Greece", "Greenland", "Grenada", "Guadeloupe", "Guam", "Guatemala", "Guernsey", "Guinea", "Guinea-Bissau", "Guyana", "Haiti", "Heard Island and McDonald Islands",
                "Honduras", "Hong Kong SAR China", "Hungary", "Iceland", "India", "Indonesia", "Iran", "Iraq", "Ireland", "Isle of Man", "Israel", "Italy", "Jamaica", "Japan", "Jersey", "Johnston Island", "Jordan", "Kazakhstan", "Kenya", "Kiribati",
                "Kuwait", "Kyrgyzstan", "Laos", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libya", "Liechtenstein", "Lithuania", "Luxembourg", "Macau SAR China", "Macedonia", "Madagascar", "Malawi", "Malaysia", "Maldives", "Mali", "Malta", "Marshall Islands",
                "Martinique", "Mauritania", "Mauritius", "Mayotte", "Metropolitan France", "Mexico", "Micronesia", "Midway Islands", "Moldova", "Monaco", "Mongolia", "Montenegro", "Montserrat", "Morocco", "Mozambique", "Myanmar [Burma]", "Namibia", "Nauru", "Nepal", "Netherlands",
                "Netherlands Antilles", "Neutral Zone", "New Caledonia", "New Zealand", "Nicaragua", "Niger", "Nigeria", "Niue", "Norfolk Island", "North Korea", "North Vietnam", "Northern Mariana Islands", "Norway", "Oman", "Pacific Islands Trust Territory", "Pakistan", "Palau", "Palestinian Territories", "Panama", "Panama Canal Zone",
                "Papua New Guinea", "Paraguay", "People's Democratic Republic of Yemen", "Peru", "Philippines", "Pitcairn Islands", "Poland", "Portugal", "Puerto Rico", "Qatar", "Romania", "Russia", "Rwanda", "Réunion", "Saint Barthélemy", "Saint Helena", "Saint Kitts and Nevis", "Saint Lucia", "Saint Martin", "Saint Pierre and Miquelon",
                "Saint Vincent and the Grenadines", "Samoa", "San Marino", "Saudi Arabia", "Senegal", "Serbia", "Serbia and Montenegro", "Seychelles", "Sierra Leone", "Singapore", "Slovakia", "Slovenia", "Solomon Islands", "Somalia", "South Africa", "South Georgia and the South Sandwich Islands", "South Korea", "Spain", "Sri Lanka", "Sudan",
                "Suriname", "Svalbard and Jan Mayen", "Swaziland", "Sweden", "Switzerland", "Syria", "São Tomé and Príncipe", "Taiwan", "Tajikistan", "Tanzania", "Thailand", "Timor-Leste", "Togo", "Tokelau", "Tonga", "Trinidad and Tobago", "Tunisia", "Turkey", "Turkmenistan", "Turks and Caicos Islands",
                "Tuvalu", "U.S. Minor Outlying Islands", "U.S. Miscellaneous Pacific Islands", "U.S. Virgin Islands", "Uganda", "Ukraine", "Union of Soviet Socialist Republics", "United Arab Emirates", "United Kingdom", "United States", "Unknown or Invalid Region", "Uruguay", "Uzbekistan", "Vanuatu", "Vatican City", "Venezuela", "Vietnam", "Wake Island", "Wallis and Futuna", "Western Sahara",
                "Yemen", "Zambia", "Zimbabwe", "Åland Islands" };
        static string[] USStates = { "Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "Florida", "Georgia", "Hawaii", "Idaho", "Illinois", "Indiana", "Iowa", "Kansas", "Kentucky", "Louisiana", "Maine", "Maryland", "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire", "New Jersey", "New Mexico", "New York", "North Carolina", "North Dakota", "Ohio", "Oklahoma", "Oregon", "Pennsylvania", "Rhode Island", "South Carolina", "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia", "Washington", "West Virginia", "Wisconsin", "Wyoming" };
        static string[] USCapitals = { "Montgomery", "Juneau", "Phoenix", "Little Rock", "Sacramento", "Denver", "Hartford", "Dover", "Tallahassee", "Atlanta", "Honolulu", "Boise", "Springfield", "Indianapolis", "Des Moines", "Topeka", "Frankfort", "Baton Rouge", "Augusta", "Annapolis", "Boston", "Lansing", "St. Paul", "Jackson", "Jefferson City", "Helena", "Lincoln", "Carson City", "Concord", "Trenton", "Santa Fe", "Albany", "Raleigh", "Bismarck", "Columbus", "Oklahoma City", "Salem", "Harrisburg", "Providence", "Columbia", "Pierre", "Nashville", "Austin", "Salt Lake City", "Montpelier", "Richmond", "Olympia", "Charleston", "Madison", "Cheyenne" };

    }


    // To avoid use of generic Exception and make code analyzer complain about it!
    [Serializable()]
    public class BonzaException : Exception
    {
        public BonzaException() { }
        public BonzaException(string message) : base(message) { }
        public BonzaException(string message, Exception innerException) : base(message, innerException) { }
        protected BonzaException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    };


    internal static class ExtensionMethods
    {
        static readonly Random rnd = new Random();

        // Randomize the order of elements in a list
        public static List<T> Shuffle<T>(this List<T> list)
        {
            if (list == null || list.Count < 2) return list;

            var shuffledList = new List<T>(list);
            for (int i = 0; i < list.Count; i++)
            {
                // It's Ok to have p1==p2, for instance when shuffling a 2-element list 
                // so that in 50% we return the original list, in 50% a swapped version
                int p1, p2;
                p1 = rnd.Next(list.Count);
                p2 = rnd.Next(list.Count);
                T temp = shuffledList[p1];
                shuffledList[p1] = shuffledList[p2];
                shuffledList[p2] = temp;
            }
            return shuffledList;
        }

        // Return a random element from a list
        public static T TakeRandom<T>(this List<T> list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (list.Count == 0)
                throw new InvalidDataException("Can't select a random element from a list of zero element");
            return list[rnd.Next(list.Count)];
        }
    }
}
