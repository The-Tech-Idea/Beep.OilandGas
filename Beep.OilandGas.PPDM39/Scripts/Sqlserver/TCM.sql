execute sp_addextendedproperty 'Description','SAMPLE ANALYSIS ACCURACY: Use this table to capture information about the accuracy of an analysis, a specific step in an analysis or the accuracy of a specific result of an analysis. You can capture the accuracy of any result by identifying the SYSTEM, TABLE and COLUMN in which the result is stored, and the PPDM_REFERENCED_GUID to identify the specific row of data that is relevant.','USER','dbo','TABLE','ANL_ACCURACY';
go

execute sp_addextendedproperty 'Description','ANALYSIS BATCH: A group of samples that are run on a particular day and time of a particular analysis. When problems with an analysis are found, it is often important to identify the batch within which the analysis was done, so other results that may be compromised may also be identified. A batch is usually created by a lab.','USER','dbo','TABLE','ANL_ANALYSIS_BATCH';
go

execute sp_addextendedproperty 'Description','ANALYSIS REPORT: Use this header table to keep track of the details about an analysis project, such as who did the analysis, when it was done and so on. An analysis report may include information from one or many analysis steps. While each step in an analysis typically is run on one and only one sample, an entire analysis report may document the addition or creation of new samples as the overall study progresses from one step to another.','USER','dbo','TABLE','ANL_ANALYSIS_REPORT';
go

execute sp_addextendedproperty 'Description','ANALYSIS STEP: Describes all of the steps included in an analysis, including both preparation steps and analysis steps. In some cases, one procedure may both prepare a sample for the next step and be used in an analysis, creating some ambiguity about whethera step is a preparation step or an analysis step. However, for this table whether the step is a preparation or analysis is irrelevent. Preparation steps may include acid washes, thin sectioning or size fractionating. Analysis steps may include pyrolysis or fractioning.','USER','dbo','TABLE','ANL_ANALYSIS_STEP';
go

execute sp_addextendedproperty 'Description','ANALYSIS BUSINESS ASSOCIATE: Use this table to keep track of all the business associates who are involved with this study or with a specific step in a study. You can track who a study was done for, who it was done by, who the technician was, what lab the study was doneby, who the scientist was and so on. Use the BA_ROLE column to specify the role(s) played by each participant. If one participant plays more than one role, use one row in the table for each role played by the BA.','USER','dbo','TABLE','ANL_BA';
go

execute sp_addextendedproperty 'Description','ANALYSIS CALCULATION ALIAS: All possible names, codes and other identifiers for the calculation can be stored here.','USER','dbo','TABLE','ANL_CALC_ALIAS';
go

execute sp_addextendedproperty 'Description','ANALYSIS CALCULATION EQUIVALENCE: This table is used to identify any equivalences between ratios or any relations between ratios.','USER','dbo','TABLE','ANL_CALC_EQUIV';
go

execute sp_addextendedproperty 'Description','ANALYSIS CALCULATION FORMULA: This table carries information about the formulas used for calculations. Information such as assigned variables for the formula or a portion of the formula, type of hydrocarbon present in the sediment, etc.','USER','dbo','TABLE','ANL_CALC_FORMULA';
go

execute sp_addextendedproperty 'Description','ANALYSIS CALCULATION METHOD: A table that lists the kinds of calculated values that are created during various kinds of analysis. In some cases, calculated values are created by the scientist studying the analysis, and in some cases calculated values are provided by an analysis laboratory, either in addition to raw values or instead of raw values. Examples include the carbon preference index (CPI), or the pristane / phytane ratio. It is not necessary to associate methods with a valid set of calculations, but this can be done if the user wishes, and mayhelp to group or retrieve calculations.','USER','dbo','TABLE','ANL_CALC_METHOD';
go

execute sp_addextendedproperty 'Description','ANALYSIS CALCULATION SET: This table is used to group together calculations that are part of a particular process, or used by a software application or laboratory. Sets can be used to aid in the collection of ratios or calculated values that belong together.','USER','dbo','TABLE','ANL_CALC_SET';
go

execute sp_addextendedproperty 'Description','ANALYSIS COAL RANK: the thermal rank of a coal sample.','USER','dbo','TABLE','ANL_COAL_RANK';
go

execute sp_addextendedproperty 'Description','ANALYSIS COAL RANK SCHEME: This table is used to explain the coal rank scheme and how it works to classify the coal rank.','USER','dbo','TABLE','ANL_COAL_RANK_SCHEME';
go

execute sp_addextendedproperty 'Description','ANALYSIS COMPONENT: use this table to keep track of the various business objects that a sample analysis may be associated with. For example, samples are collected from well tests, production volumes at a valve or tank, at an HSE incident, etc.','USER','dbo','TABLE','ANL_COMPONENT';
go

execute sp_addextendedproperty 'Description','ANALYSIS DETAIL: Use this table to store any study result in vertical form. Results from many kinds of studies are intended to be captured in tables that are specific for that kind of study. However, if a specific result is not supported in other tables, they may be stored here, with care given to ensure high quality results. We recommend that these exceptions be reported to the PPDM Association so that standard tables for new study types can be created. Storing all results in this table may lead to very large table sizes, inconsistent data storage, data quality challenges and retrieval or query problems.','USER','dbo','TABLE','ANL_DETAIL';
go

execute sp_addextendedproperty 'Description','ANALYSIS ELEMENTAL: This table should be used to capture the different elements present in the sample. For example, the ash, carbon, hydrogen, nickel, etc. Also, the quantity of these elements, the ratio between elements and the step sequence number in which the element quantity is uncovered.','USER','dbo','TABLE','ANL_ELEMENTAL';
go

execute sp_addextendedproperty 'Description','ANALYSIS ELEMENTAL DETAIL: Use this table to capture specific information that is captured on an element, or about the element. Where specific columns exist for commonly used information, please use them. This table covers exceptions not handled by PPDM.','USER','dbo','TABLE','ANL_ELEMENTAL_DETAIL';
go

execute sp_addextendedproperty 'Description','ANALYSIS EQUIPMENT: This table is used to keep track of the equipment that is used in particular steps, the role of the equipment, if there are any problems with the equipment, or any information about how the equipment was calibrated. The column EQUIP_ID is used toidentify actual equipment being used while the column CATALOGUE_EQUIP_ID is used to identify the different possible equipments available.','USER','dbo','TABLE','ANL_EQUIP';
go

execute sp_addextendedproperty 'Description','ANALYSIS FLOURESCENCE: This table would capture fluorescence from other kinds of studies, such as QGFE, QGF, TSF, UVF. While their are differences in the methods used to create the data (and these methods are crucial to understanding the data), the data that is created is of the same type.','USER','dbo','TABLE','ANL_FLUOR';
go

execute sp_addextendedproperty 'Description','GAS ANALYSIS: The Well Gas Analysis table contains information identifying the location in a well bore where a normal gas sample was obtained to determine the hydrocarbon composition from an gas analysis.','USER','dbo','TABLE','ANL_GAS_ANALYSIS';
go

execute sp_addextendedproperty 'Description','ANALYSIS GAS CHROMATOGRAPHY: An essential technique used to separate and quantify the constituents of a mixture. A gas is passed through a column either packed with a solid or filled with a liquid; the chromatographic systems are known as gsc and glc, respectively.','USER','dbo','TABLE','ANL_GAS_CHRO';
go

execute sp_addextendedproperty 'Description','ANALYSIS GAS COMPOSITION: This table lists the components of the gas sample, and the amount of each substance identified.','USER','dbo','TABLE','ANL_GAS_COMPOSITION';
go

execute sp_addextendedproperty 'Description','ANALYSIS GAS DETAIL: The Well Gas Analysis Detail table contains the results of an analysis for the hydrocarbon composition of a normal gas sample, such as mole percentages of CO2, HE, H2S, etc.','USER','dbo','TABLE','ANL_GAS_DETAIL';
go

execute sp_addextendedproperty 'Description','ANALYSIS GAS HEAT CONTENT: stores information about the heating content of a gas sample. The heat content is often expressed as gross heating value.','USER','dbo','TABLE','ANL_GAS_HEAT';
go

execute sp_addextendedproperty 'Description','ANALYSIS GAS PRESSURE: records information about the pressures and temperatures at which a gas sample is measured analyzed at. Pressures may be measured for example as the separator, treater or as received at the lab. Typically measurements are taken in temperature and pressure pairs.','USER','dbo','TABLE','ANL_GAS_PRESS';
go

execute sp_addextendedproperty 'Description','ANALYSIS ISOTOPE: Use this table to store information about the isotope analysis such as the identifier for the standard used in the isotope analysis, whether or not delta notation is used, etc.','USER','dbo','TABLE','ANL_ISOTOPE';
go

execute sp_addextendedproperty 'Description','ANALYSIS ISOTOPE STANDARD: Use this table to store information about the standard used in the isotope analysis such as the identifier for the standard used in the isotope analysis, the standard name, etc.','USER','dbo','TABLE','ANL_ISOTOPE_STD';
go

execute sp_addextendedproperty 'Description','ANALYSIS LIQUID CHROMATOGRAPHY: An essential technique used to separate and quantify the constituents of a mixture. A Material is passed through a column filled with silicanite using various hydrocarbons as solvents. Solvents will each carry a range of materialsthrough the column. This method is usually used to create a fraction of a sample, such as aromatic components or saturates. These fractions can then be put into a gas chromatographic analysis for detailed analysis.','USER','dbo','TABLE','ANL_LIQUID_CHRO';
go

execute sp_addextendedproperty 'Description','SAMPLE ANALYSIS GAS CHROMATOGRAPHY: An essential technique used to separate and quantify the constituents of a mixture. A gas is passed through a column either packed with a solid or filled with a liquid; the chromatographic systems are known as gsc and glc, respectively.','USER','dbo','TABLE','ANL_LIQUID_CHRO_DETAIL';
go

execute sp_addextendedproperty 'Description','ANALYSIS MACERAL: The analysis of the organic component of a rock sample identifying the microscopic constituents of the sample according to the morphology and the reflectance. The type and parameters of the maceral sample can be determined.','USER','dbo','TABLE','ANL_MACERAL';
go

execute sp_addextendedproperty 'Description','ANALYSIS MACERAL MATURITY: Maceral maturity can be determined by vitrinite reflectance. The vitrinite reflectance gives information about the chemical composition of the sample. The type of sample can be determined as well, this allows for a sample quality description as well.','USER','dbo','TABLE','ANL_MACERAL_MATURITY';
go

execute sp_addextendedproperty 'Description','ANALYSIS METHOD: This table lists the methods or steps that are included in a typical analysis. Each sample analysis should be associated with a method; if more than one method is used (often in series), each step should be stored with the method used. A method may be a preparation step or a step that includes taking measurements. In some kinds of studies, the method may both prepare a sample for a subsequent step and be used to take various measurements. This table is associated with additional method description tables, all named ANL_METHOD_. This tableis also associated with a number of tables prefixed ANL_QC_. The QC tables are used to store valid parameters, settings and other information for each method, and may be used to help quality control incoming data as it is received.','USER','dbo','TABLE','ANL_METHOD';
go

execute sp_addextendedproperty 'Description','ANALYSIS METHOD ALIAS: All possible names, codes and other identifiers can be stored here.','USER','dbo','TABLE','ANL_METHOD_ALIAS';
go

execute sp_addextendedproperty 'Description','ANALYSIS METHOD EQUIVALENCE: Use this table to show how various methods relate to each other. This is valuable, particularly when labs may execute a slight variation on a methodology that is very comparable to a method by another lab, or when a new methodology supersedes an older one.','USER','dbo','TABLE','ANL_METHOD_EQUIV';
go

execute sp_addextendedproperty 'Description','ANALYSIS METHOD PARAMETER: Use this table to capture typical or accepted parameters for any analysis method. While this table may list a variety of parameter settings for each method, an actual analysis study will use only one. For example, several carrier gases may be acceptable for Gas Spectroscopy, but only one of these will be used in a study.','USER','dbo','TABLE','ANL_METHOD_PARM';
go

execute sp_addextendedproperty 'Description','ANALYSIS METHOD SET: this table lists groups or sets of analysis activities that may be used in a specific kind of study. For example, organic geochemical studies may use a set of standardized steps (such a grinding, slicing, pyrolysis, immersion etc) each timea study is undertaken, (even though not all steps may be used for every study).','USER','dbo','TABLE','ANL_METHOD_SET';
go

execute sp_addextendedproperty 'Description','ANALYSIS OIL ANALYSIS: Well Oil Analysis table contains information identifying the location and results of an oil sample obtained to determine the hydrocarbon composition of an oil analysis. This table contains results from several analysis methods (describedin ANL_ANALYSIS_STEP), combined into a single table for ease of reporting.','USER','dbo','TABLE','ANL_OIL_ANALYSIS';
go

execute sp_addextendedproperty 'Description','ANALYSIS OIL DETAIL: The Analysis Detail table contains the results of an analysis for the composition of an oil sample.','USER','dbo','TABLE','ANL_OIL_DETAIL';
go

execute sp_addextendedproperty 'Description','ANALYSIS OIL DISTILLATION: This table is used to keep track of the physical properties surrounding the sample during and after distillation. For example, temperature, pressure, starting boiling point temperature, etc.','USER','dbo','TABLE','ANL_OIL_DISTILL';
go

execute sp_addextendedproperty 'Description','ANALYSIS OIL FRACTION: This table is used to keep track of the fraction that is distilled from the sample. Information such as the volume of fraction distilled, the temperature of distillation, the sequence numbers that help keep track of when something is happening, etc.','USER','dbo','TABLE','ANL_OIL_FRACTION';
go

execute sp_addextendedproperty 'Description','ANALYSIS OIL VISCOSITY: Well Oil Viscosity table contains information pertaining to the viscosity (the property of a substance to offer internal resistance to flow) of an oil sample obtained for an oil analysis.','USER','dbo','TABLE','ANL_OIL_VISCOSITY';
go

execute sp_addextendedproperty 'Description','ANALYSIS PALYNOLOGY: This table is used to store information about the amount of woody material seen through transmitted light. The analysis includes the study of microscopic bodies known as palynomorphs which include a wide array of organic entities.','USER','dbo','TABLE','ANL_PALEO';
go

execute sp_addextendedproperty 'Description','ANALYSIS PALEO MATURITY: Palynology is the science that studies contemporary and fossil palynomorphs, including pollen, spores, dinoflagellate cysts, acritarchs, chitinozoans and scolecodonts, together with particulate organic matter (POM) and kerogen found in sedimentary rocks and sediments. Palynology does not include diatoms, foraminiferans or other organisms with silicaceous or calcareous exoskeletons.','USER','dbo','TABLE','ANL_PALEO_MATURITY';
go

execute sp_addextendedproperty 'Description','ANALYSIS PARAMETERS: Use this table to capture the parameters used in this analysis step. If you are using the table ANL METHOD PARM, you may then compare the actual parameters with the parameters that are standardized or approved for this method. This vertical table can be used to capture information such as the solvent used, distillation temperatures and pressures, vessel sizes and more. This table is created as a vertical table because of the very large and variable number of method sets to be supported. Please use the VERTICAL TABLES support method to maximize the quality of data in this table.','USER','dbo','TABLE','ANL_PARM';
go

execute sp_addextendedproperty 'Description','ANALYSIS PROBLEM: this table contains a summary of the kinds of problems found during this step, either in preparation or analysis. Problems may be related to the entire study, a step in the study or a specific result of a study. To identify a specific result, point to the relevant SYSTEM, TABLE, COLUMN and REFERENCED PPDM GUID.','USER','dbo','TABLE','ANL_PROBLEM';
go

execute sp_addextendedproperty 'Description','ANALYSIS PYROLYSIS: Pyrolysis is the chemical analysis of a substance by heating in the absence of oxygen. Use this table to store the data surrounding the pyrolysis such as the maximum temperature, reported hydrogen index, total organic carbon, etc.','USER','dbo','TABLE','ANL_PYROLYSIS';
go

execute sp_addextendedproperty 'Description','ANALYSIS QUANTITATIVE GRAIN FLUORESCENCE AND TOTAL SCANNING FLUORESCENCE: QGF is a technique which allows for quantitative analysis of hydrocarbon on grain surface. After reservoir grains are cleaned (solvent, hydrogen peroxide and acid) the fluorescence emission spectra are measured. QGF can give information such as oil inclusion abundance, hydrocarbon concentration, and hydrocarbon on the grain surface. TSF is a semi-quantitative technique which allows for detection of aromatic compounds is sediments.','USER','dbo','TABLE','ANL_QGF_TSF';
go

execute sp_addextendedproperty 'Description','ANALYSIS REMARK: Use this table to capture narrative remarks about the analysis. Associate remarks with columns and values by using the SYSTEM, TABLE, COLUMN and REFERENCED PPDM GUID columns.','USER','dbo','TABLE','ANL_REMARK';
go

execute sp_addextendedproperty 'Description','ANALYSIS REPORT ALIAS: this this table to capture all names, codes and identifiers assigned to an analysis, such as a lab report number or an identifier assigned to an analysis in another database or system. This table can be used to help integrate multiple systems together by mapping identifiers in other systems.','USER','dbo','TABLE','ANL_REPORT_ALIAS';
go

execute sp_addextendedproperty 'Description','ANALYSIS SAMPLE: This table lists the samples that are used in an analysis. In some cases a single sample will be used, but in others new samples may be input or created at various stages of the study. Each individual step in a study is made with one sample only.To describe how portions of an existing sample are taken and used to create a new sample (often in conjunction with other sub samples) please use the table SAMPLE ORIGIN.','USER','dbo','TABLE','ANL_SAMPLE';
go

execute sp_addextendedproperty 'Description','ANALYSIS STEP CROSS REFERENCE: Use this table to relate steps to each other, often to indicate the order in which steps are run, or which steps are run concurrently (in the same analysis device, usually).','USER','dbo','TABLE','ANL_STEP_XREF';
go

execute sp_addextendedproperty 'Description','ANALYSIS TABLE RESULT: This table is provided to serve as a table of contents that lists which tables in PPDM contain the results from any study. If you populate this table, you may include it in your queries in order to avoid inefficient table joins or overly complex queries. By knowing which tables have results, you may restrict queries to the specific tables.','USER','dbo','TABLE','ANL_TABLE_RESULT';
go

execute sp_addextendedproperty 'Description','ANALYSIS QUALITY CONTROL - VALID BUSINESS ASSOCIATES: This table captures a list of business associates who are capable of carrying out the procedures described in ANL METHOD. This table is intended to be used to validate incoming data.','USER','dbo','TABLE','ANL_VALID_BA';
go

execute sp_addextendedproperty 'Description','ANALYSIS QUALITY CONTROL - VALID EQUIPMENT: This table lists the kinds of equipment that are capable of conducting the methodology described in ANL METHOD. Use this table to ensure that appropriate types of equipment are listed for a study. The table ANL QC TOLERANCE captures the tolerances for each type of equipment for the kinds of method that is employed. This table is intended to be used for quality controlling incoming data.','USER','dbo','TABLE','ANL_VALID_EQUIP';
go

execute sp_addextendedproperty 'Description','ANALYSIS QUALITY CONTROL - VALID MEASUREMENTS: This table captures the valid range of measurements that may be captured during a particular method of analysis. Typically these ranges are provided on a substance by substance basis. Note that tolerances for specific equipment are stored in ANL QC TOLERANCE. This table indicates the valid maximun, minimum range of values for specific products and substances. Intended to be used to quality control incoming data.','USER','dbo','TABLE','ANL_VALID_MEASURE';
go

execute sp_addextendedproperty 'Description','ANALYSIS VALID PROBLEMS: This table lists the kinds of problems that may arise for each valid method during a study. It also shows how severely the problem may affect the results of a study and what resolutions are possible for that problem.','USER','dbo','TABLE','ANL_VALID_PROBLEM';
go

execute sp_addextendedproperty 'Description','ANALYSIS QUALITY CONTROL - VALID TABLE RESULT: Use this table to list the PPDM (or other) tables where the results for this method should be stored. This table is intended to help quality control results, so that data is stored in the tables where it should be stored.','USER','dbo','TABLE','ANL_VALID_TABLE_RESULT';
go

execute sp_addextendedproperty 'Description','ANALYSIS QUALITY CONTROL - VALID TOLERANCE: This table lists the valid tolerances for equipment that is used in a method, including how values that exceed tolerances are represented in output data. This table is intended to be used to quality control incoming data.','USER','dbo','TABLE','ANL_VALID_TOLERANCE';
go

execute sp_addextendedproperty 'Description','WATER ANALYSIS: The Water Analysis table contains information identifying the location and type of water sample obtained, such as samples obtained from the wellbore. Includes depth, salinity, resistivity, density and temperature.','USER','dbo','TABLE','ANL_WATER_ANALYSIS';
go

execute sp_addextendedproperty 'Description','WATER ANALYSIS DETAIL: The Water Analysis Detail table contains detailed information about the composition and physical properties of a water sample being analyzed. Properties may consist of a concentration of dissolved solids su ch as Sodium (Na), Calcium (Ca), or Magnesium (Mg) etc.','USER','dbo','TABLE','ANL_WATER_DETAIL';
go

execute sp_addextendedproperty 'Description','ANALYSIS WATER SALINITY: Use this table to record the total dissolved solids (salinity) of a water sample as calculated or as measured at specific temperatures.','USER','dbo','TABLE','ANL_WATER_SALINITY';
go

execute sp_addextendedproperty 'Description','APPLICATION: Applications made for authority or permission, such as for extensions or continuations to the primary term of the agreement or for licenses.','USER','dbo','TABLE','APPLICATION';
go

execute sp_addextendedproperty 'Description','APPLICATION COMPONENT: This table is used to capture the relationships between applications and busines objects, such as wells, equipment, documents, seismic sets and contracts.','USER','dbo','TABLE','APPLICATION_COMPONENT';
go

execute sp_addextendedproperty 'Description','APPLICATION ALIAS: The Name Alias table stores multiple alias names for a given application. Every name, code and identifier assigned to an application should be stored in this table. Mark the one you prefer to use using the PREFERRED IND, and the application that an alias is suitable for using the APPLICATION IDENTIFIER.','USER','dbo','TABLE','APPLIC_ALIAS';
go

execute sp_addextendedproperty 'Description','APPLICATION AREA: A list of the areas into which an application falls.','USER','dbo','TABLE','APPLIC_AREA';
go

execute sp_addextendedproperty 'Description','APPLICATION ATTACHMENT: Describes the attachements to the application, such as letters, maps and so on.','USER','dbo','TABLE','APPLIC_ATTACH';
go

execute sp_addextendedproperty 'Description','APPLICATION BUSINESS ASSOCIATE: This table is used to capture information about who was involved in an application, what role that person played in the application (approver, creator, reviewer etc.) and when they were involved.','USER','dbo','TABLE','APPLIC_BA';
go

execute sp_addextendedproperty 'Description','APPLICATION DESCRIPTION: A list of the descriptive details about an application. For an application for a license, could describe details about the proposed operations. This table is vertical to support the extremely wide range of descriptions possible, depending on the type of application and the company or agency you are doing business with. The Primary key is two part, to allow specific description types to be associated with specific application types.','USER','dbo','TABLE','APPLIC_DESC';
go

execute sp_addextendedproperty 'Description','APPLICATION REMARK: Narrative remarks about the application, the decision made about the application and events during the application process.','USER','dbo','TABLE','APPLIC_REMARK';
go

execute sp_addextendedproperty 'Description','AREA: Describes geographic areas of any type, such as projects, offshore areas etc.','USER','dbo','TABLE','AREA';
go

execute sp_addextendedproperty 'Description','AREA ALIAS: Areas may have more than one name. Variations can be stored here.','USER','dbo','TABLE','AREA_ALIAS';
go

execute sp_addextendedproperty 'Description','AREA CLASS: Use this table to define the hierarchy of areas within a specific scheme.  A typical scheme would be for geographic hierarchies.  You can have a hierarchy for each kind of system being defined.','USER','dbo','TABLE','AREA_CLASS';
go

execute sp_addextendedproperty 'Description','AREA COMPONENT: This table is used to capture the relationships between areas and busines objects, such as wells, equipment, documents, seismic sets and contracts.','USER','dbo','TABLE','AREA_COMPONENT';
go

execute sp_addextendedproperty 'Description','AREA CONTAIN: Describes the overlap or containment relationship bewteen areas. For example, AREA 1 may fully or paritally contain AREA 2. AREA 2 may overlap AREA 3.','USER','dbo','TABLE','AREA_CONTAIN';
go

execute sp_addextendedproperty 'Description','AREA DESCRIPTION: Allows an area to be described using textual remarks or codified descriptors.','USER','dbo','TABLE','AREA_DESCRIPTION';
go

execute sp_addextendedproperty 'Description','AREA HIERARCHY:  Use this table to identify the various kinds of hierarchy you use.  For example, you can set up a hierarchy for US systems (country, state, county) or for Canada (country, province), or any other hierarchy you use.','USER','dbo','TABLE','AREA_HIERARCHY';
go

execute sp_addextendedproperty 'Description','AREA HIERARCHY DETAIL:  This table defines which kinds of areas exist at each level.  Use this table to create a template for the hierarchy. For example, COUNTRY might be level 1, and STATE might be level 2.   Use AREA_CLASS to create the actual hierarchy (AREA CLASS contains the names of the countries, states etc).','USER','dbo','TABLE','AREA_HIER_DETAIL';
go

execute sp_addextendedproperty 'Description','AREA CROSS REFERENCE: Use this table to relate areas to each other for reasons other than containment or overlap relationships (these should be managed in AREA_CONTAIN). Relationships change over time (and you may need to capture historical relationships) or you may cross reference areas to each other for more than one reason.','USER','dbo','TABLE','AREA_XREF';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE ADDRESS: The Business Associate Address table contains information on the address, phone numbers, primary contacts, and location of the business associate, allowing clients to have multiple addresses. For example, compan ies that have a headquarters and various satellite offices.','USER','dbo','TABLE','BA_ADDRESS';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE NAME ALIAS: The Business Associate Name Alias table stores multiple alias names for a given business associate name. For example, the company name "Petroleum Information" may have several different spellings, such as, Pe troleum Info, P.I., etc. This table allows the system to translate all the multiple names into one common name.','USER','dbo','TABLE','BA_ALIAS';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE AUTHORITY: Describes the authority held by a business associate to make payments, sign contracts etc. Considered in a business context. Application or database authorities are held in ENTITLEMENTS.','USER','dbo','TABLE','BA_AUTHORITY';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE AUTHORITY COMPONENT: this table is used to keep track of the business objects over which a BA has authority of some type (usually financial or signing).','USER','dbo','TABLE','BA_AUTHORITY_COMP';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE COMPONENT: This table is used to capture the relationships between business associates and busines objects, such as wells, equipment, documents, seismic sets and contracts.','USER','dbo','TABLE','BA_COMPONENT';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE CONSORTIUM SERVICE: A service that is provided to a consortium by another business associate.','USER','dbo','TABLE','BA_CONSORTIUM_SERVICE';
go

execute sp_addextendedproperty 'Description','BA CONTACT INFORMATION: represents the contact information for a company. May be a phone number, fax number, EMail address, Web URL etc.','USER','dbo','TABLE','BA_CONTACT_INFO';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE CREW: This table can be used to track crews that do work for an organization. A crew may consist of members from one or more companies. Each crew may be assigned to a support facility (such as a rig or a vessel).','USER','dbo','TABLE','BA_CREW';
go

execute sp_addextendedproperty 'Description','BA CREW MEMBER: Use this table to track members of a crew at any given point in time. A crew member may be an individual or a company. The history of crew members may be tracked in this table, with currently active members indicated.','USER','dbo','TABLE','BA_CREW_MEMBER';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE DESCRIPTION: use this table to capture descriptive details about a business associate where that information is not supported by the rest of the BA module. This table was created to allow members to add specialized values to their implementation, or as a place to store information until a PPDM model extension is created.','USER','dbo','TABLE','BA_DESCRIPTION';
go

execute sp_addextendedproperty 'Description','BA EMPLOYEE: Defines how each company has many staff, each person may work for many companies. Each person my work for a company many times, with different positions.','USER','dbo','TABLE','BA_EMPLOYEE';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE LICENSE: An approval or authorization to conduct operations that are not directly associated with seismic, wells or facilities. Could include general ground surveys, aeromag surveys, field stratigraphy and so on.','USER','dbo','TABLE','BA_LICENSE';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE LICENSE NAME ALIAS: The Name Alias table stores multiple alias names for a given license name.','USER','dbo','TABLE','BA_LICENSE_ALIAS';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE LICENSE AREA: list of the areas into which a business associate license falls.','USER','dbo','TABLE','BA_LICENSE_AREA';
go

execute sp_addextendedproperty 'Description','BA LICENSE CONDITION: lists the conditions under which the license or approval has been granted. May include payment of fees, development of agreements, performance of work etc. If desired, the project module may be used to track fulfillment of operational conditions. The obligations module is used to track payment of fees.','USER','dbo','TABLE','BA_LICENSE_COND';
go

execute sp_addextendedproperty 'Description','LICENSE CONDITION CODE: A list of valid condition codes for a type of condition that can be placed on a license. For example, a report may be required or not required.','USER','dbo','TABLE','BA_LICENSE_COND_CODE';
go

execute sp_addextendedproperty 'Description','LICENSE CONDITION: A list of valid condition types that can be placed on a license. These conditions may require activities, payments, reports, time deadlines etc. Management of these conditions may be undertaken through the OBLIGATION or PROJECT Modulesas appropriate to your business.','USER','dbo','TABLE','BA_LICENSE_COND_TYPE';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE LICENSE REMARK: a text description to record general comments on the license tracking when remark was made, who is the author and the type of remark.','USER','dbo','TABLE','BA_LICENSE_REMARK';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE LICENSE STATUS: Tracks the status of well license throughout its lifetime. Various types of status may be included at the discretion of the implementor.','USER','dbo','TABLE','BA_LICENSE_STATUS';
go

execute sp_addextendedproperty 'Description','LICENSE TYPE: The type of license that has been granted, such as an activity licenes to produce, flare etc. In some jurisdicitons a single license may be granted to cover all operations, in others seperate licenses are granted based on the type of operation.','USER','dbo','TABLE','BA_LICENSE_TYPE';
go

execute sp_addextendedproperty 'Description','BA LICENSE VIOLATION: Use this table to track incidents where the terms of a license have been violated (or perhaps are claimed to be violated). At this time the table is relatively simple in content.','USER','dbo','TABLE','BA_LICENSE_VIOLATION';
go

execute sp_addextendedproperty 'Description','BA ORGANIZATION: allows the internal corporate structure of a business associate to be tracked at whatever level is appropriate to the user site. Connections to this table are not generally provided in PPDM, but can be made as extensions at user sites basedon business needs at each site.','USER','dbo','TABLE','BA_ORGANIZATION';
go

execute sp_addextendedproperty 'Description','BA ORGANIZATION COMPONENT: allows relationships in internal corporate structure of a business associate to be tracked at whatever level is appropriate to the user site.','USER','dbo','TABLE','BA_ORGANIZATION_COMP';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE PERMIT: Describes the permits held by a business associate to conduct various operations in different jurisdictions. Permits may be held for well drilling operations, seismic operations etc. Could also include certifications, such as those from professional associattions or educational institutions.','USER','dbo','TABLE','BA_PERMIT';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE PREFERENCE: This set of tables may be used to track the preferences of a user for application settings, negotiation environments, meeting times or places, report format types or anything else you can think of.','USER','dbo','TABLE','BA_PREFERENCE';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE PREFERENCE LEVEL: Use this table to rank specific preferences in order of desirability.','USER','dbo','TABLE','BA_PREFERENCE_LEVEL';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE SERVICE: Describes the primary services provided by a business associate. For example drilling contractor, logging com pany, seismic broker etc.','USER','dbo','TABLE','BA_SERVICE';
go

execute sp_addextendedproperty 'Description','BA SERVICE ADDRESS: a cross reference which allows a connection between the services provided by a business associate and the addresses at which that service is provided.','USER','dbo','TABLE','BA_SERVICE_ADDRESS';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE CROSS REFERENCE: represents historical connections between business associates, such as mergers, buy outs, name changes, amalgamations, etc.','USER','dbo','TABLE','BA_XREF';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE: The Business Associate table serves as a validation/lookup table associating the code values for each business associate with their full name and information about company partners and other parties with whom business is conducted (e.g., oil companies, applicants, owners, contractors, operators, original operators, previous operators, etc.). BUSINESS ASSOCIATE COMPANY: A valid sub-type of BUSINESS ASSOCIATE. BUSINESS ASSOCIATE CONSORTIUM: A valid subtype of BUSINESS ASSOCIATE that is a consortium composed of otherBUSINESS ASSOCIATES. Members of the consortium, with their interest and roles in it, are tracked as an INTEREST SET. BUSINESS ASSOCIATE GOVERNMENT: A valid sub type of BUSINESS ASSOCIATE that is a governmenta, regulaltory or jurisdictional body. BUSINESS ASSOCIATE PERSON: A valid sub-type of BUSINESS ASSOCIATE that is an individual person. Relationships of the person to a company or jurisdiction or organizational structure may be captured in the table BA XREF.','USER','dbo','TABLE','BUSINESS_ASSOCIATE';
go

execute sp_addextendedproperty 'Description','CATALOGUE ADDITIVE: Use this table and its children to list and describe the kinds of additives that you need. Think of this as a catalogue or brochure that shows all the kinds of additive that you may use, but may or may not actually have. this table includes drillilng additives and materials to be used in operations such as hydraulic fracturing.','USER','dbo','TABLE','CAT_ADDITIVE';
go

execute sp_addextendedproperty 'Description','CATALOGUE ADDITIVE ALIAS: Additives that is listed in catalogues may have more than one name, code or identifier, particularly if it is distributed by more than one vendor. Care should be taken to ensure that these listings are actually for the same productand not similar products. All possible names, codes and other identifiers can be stored here.','USER','dbo','TABLE','CAT_ADDITIVE_ALIAS';
go

execute sp_addextendedproperty 'Description','CATALOGUE ADDITIVE GROUP PART: Use this table to associate Catalogue Additives to a group referenced in CAT_ADDITIVE_GROUP.','USER','dbo','TABLE','CAT_ADDITIVE_ALLOWANCE';
go

execute sp_addextendedproperty 'Description','CATALOGUE ADDITIVE GROUP: Unique Additive groups may be created to associate additives with similar function, or that are prohibited for use in a particular jurisdiction.','USER','dbo','TABLE','CAT_ADDITIVE_GROUP';
go

execute sp_addextendedproperty 'Description','CATALOGUE ADDITIVE GROUP PART: Use this table to associate Catalogue Additives to a group referenced in CAT_ADDITIVE_GROUP.','USER','dbo','TABLE','CAT_ADDITIVE_GROUP_PART';
go

execute sp_addextendedproperty 'Description','CATALOGUE ADDITIVE SPECIFICATIONS: Use this table to capture the published specifications for kinds of additives, especially the composition of the additive.','USER','dbo','TABLE','CAT_ADDITIVE_SPEC';
go

execute sp_addextendedproperty 'Description','CATALOGUE ADDITIVE TYPE: A material added to a fluid to perform one or more specific functions, such as a weighting agent, viscosifier or lubricant. (Schlumberger Oilfield Glossary). Note that the function of this table may also be assumed by the CLASSIFICATION module for more robust and complete classifications.','USER','dbo','TABLE','CAT_ADDITIVE_TYPE';
go

execute sp_addextendedproperty 'Description','ADDITIVE CATALOGUE CROSS REFERENCE: Use this table to list relationships between additives. For example, a new additive may be developed to replace an older product, or two products may be equivalent.','USER','dbo','TABLE','CAT_ADDITIVE_XREF';
go

execute sp_addextendedproperty 'Description','EQUIPMENT CATALOGUE: Use this table and its children to list and describe the kinds of equipment that you need. Think of this as a catalogue or brochure that shows all the kinds of equipment that you may use, but may or may not actually have. Actual peices of equipment that exist are defined in the table EQUIPMENT.','USER','dbo','TABLE','CAT_EQUIPMENT';
go

execute sp_addextendedproperty 'Description','CATALOGUE EQUIPMENT ALIAS: Equipment that is listed in catalogues may have more than one name, particularly if it is distributed by more than one vendor. Care should be taken to ensure that these listings are actually for the same equipment and not similar equipment. All possible names, codes and other identifiers can be stored here.','USER','dbo','TABLE','CAT_EQUIP_ALIAS';
go

execute sp_addextendedproperty 'Description','EQUIPMENT CATALOGUE SPECIFICATIONS: Use this table to capture the published specifications for kinds of equipment, such as lengths, diameters, weights and so on.','USER','dbo','TABLE','CAT_EQUIP_SPEC';
go

execute sp_addextendedproperty 'Description','CLASSIFICATION LEVEL: This table is used to capture the levels in classification systems, such as those that describe types of equipment. A number of classification schemes are availabe, including the UNSPSC code set. Typically, these classification systems are hierarchical. Objects may be classified at any level of the classification system, and through more than one classification system.','USER','dbo','TABLE','CLASS_LEVEL';
go

execute sp_addextendedproperty 'Description','CLASSIFICATION LEVEL ALIAS: An alternate name, code or identifier for a classification level. We recommend that all names, codes and identifiers be stored in this table and denormalized elsewhere as required by performance or other issues. You may also use this table to identify classification or granularity and hierarchies among values in a reference table.','USER','dbo','TABLE','CLASS_LEVEL_ALIAS';
go

execute sp_addextendedproperty 'Description','CLASSIFICATION LEVEL COMPONENT: This table is used to capture the relationships between specific levels of the classification systems and busines objects, such as wells, equipment, documents, seismic sets and land rights. You can also use Classification Systems to embed hierarchies into reference tables, by indicating the name of the reference table that has been classified. In this case, the values in the Classification system should correspond to the values in the reference table (see CLASS LEVEL ALIAS).','USER','dbo','TABLE','CLASS_LEVEL_COMPONENT';
go

execute sp_addextendedproperty 'Description','CLASSIFICATION LEVEL DESCRIPTIONS: Use this table to define what kinds of descriptions are relevant for objects at a level in the classification system. For example, use this table to describe the range of sizes for tubing classification etc.','USER','dbo','TABLE','CLASS_LEVEL_DESC';
go

execute sp_addextendedproperty 'Description','REFERENCE CLASSIFICATION LEVEL TYPE: The type of level that has been assigned in the classification system. Typically, these levels are assigned names. In the UPSPSC code set, the parent level is termed the Segment, with subordinate levels family, class and commodity. In other systems, the level may be named COUNTRY, BUSINESS UNIT etc.','USER','dbo','TABLE','CLASS_LEVEL_TYPE';
go

execute sp_addextendedproperty 'Description','CLASSIFICATION SYSTEM CROSS REFERENCE: This table may be used to indicate relationships between levels of a classification system, such as to establish similarity, granularity, overlap or equivalence in content, or to indicate the parent(s) of a level.','USER','dbo','TABLE','CLASS_LEVEL_XREF';
go

execute sp_addextendedproperty 'Description','CLASSIFICATION: SYSTEM: Identifies and describes the classification system that is used. The UNSPSC code set is a useful and practical source of classification information.','USER','dbo','TABLE','CLASS_SYSTEM';
go

execute sp_addextendedproperty 'Description','CLASSIFICATION SYSTEM ALIAS: An alternate name, code or identifier for a classification system. We recommend that all names, codes and identifiers be stored in this table and denormalized elsewhere as required by performance or other issues.','USER','dbo','TABLE','CLASS_SYSTEM_ALIAS';
go

execute sp_addextendedproperty 'Description','CLASSIFICATION SYSTEM CROSS REFERENCE: Use this table to capture the relationships between classification systems, such as replacements, enhancements, or systems with more (or less) detail.','USER','dbo','TABLE','CLASS_SYSTEM_XREF';
go

execute sp_addextendedproperty 'Description','CONSENT: Consents grant permission to conduct operations based on approval of what is done or proposed by another. Conditions, including actions to be taken or fees to be paid, may or may not be applied against the consent. Could include road use agreements, trapperconsents, land owner consents etc.','USER','dbo','TABLE','CONSENT';
go

execute sp_addextendedproperty 'Description','CONSENT BUSINESS ASSOCIATE: This table is added to allow people or companies to be associated with the consent. These people or companies may be validated against the business associates table or simple stored in an unvalidated column depending what you need to dowith the data.','USER','dbo','TABLE','CONSENT_BA';
go

execute sp_addextendedproperty 'Description','CONSENT COMPONENT: use this table to associate the land rights, seismic sets, facilities, support facilities etc that are affected by the consent.','USER','dbo','TABLE','CONSENT_COMPONENT';
go

execute sp_addextendedproperty 'Description','CONSENT CONDITIONS: conditions that are attached to the consent, such as closing fences, making reports etc. Can result in an obligation to be fulfilled.','USER','dbo','TABLE','CONSENT_COND';
go

execute sp_addextendedproperty 'Description','CONSENT REMARK: remarks about the consent and the process of obtaining it. Can be used to track progress notes and comments.','USER','dbo','TABLE','CONSENT_REMARK';
go

execute sp_addextendedproperty 'Description','CONSULTATION: This table is used to capture the process of consultation through the life cycle of business objects (cradle to grave). Consultation often occurs in order to develop agreements about how field operations should be undertaken and may involve E and P companies, regulatory agencies and various local organizations or residents.','USER','dbo','TABLE','CONSULT';
go

execute sp_addextendedproperty 'Description','CONSULTATION BUSINESS ASSOCIATE: Use this table to keep track of all the parties involved in a consultation over time.','USER','dbo','TABLE','CONSULT_BA';
go

execute sp_addextendedproperty 'Description','CONSULTATION COMPONENT: Use this table to track the business objects and other components that are related to a consultation, such as seismic to be shot, facilities to be built, roads to be accessed etc.','USER','dbo','TABLE','CONSULT_COMPONENT';
go

execute sp_addextendedproperty 'Description','CONSULTATION DISCUSSION: Use this table to keep track of the discussions that occur during a consultation. these discussions could be meetings, phone calls, electronic communication, mail and so on.','USER','dbo','TABLE','CONSULT_DISC';
go

execute sp_addextendedproperty 'Description','CONSULTATION DISCUSSION BUSINESS ASSOCIATES: Use this table to keep track of all the business associates who are involved in each discussion.','USER','dbo','TABLE','CONSULT_DISC_BA';
go

execute sp_addextendedproperty 'Description','CONSULTATION DISCUSSION ISSUE: Use this table to keep track of the issues that are rasied as part of the consultation, and the discussions at which these were reviewed or resolved etc.','USER','dbo','TABLE','CONSULT_DISC_ISSUE';
go

execute sp_addextendedproperty 'Description','CONSULTATION ISSUE: this table is used to track details about the consultation process. Each row may relate to the entire consultation or to a specific discussion related to that consultation and identifies and issue and its resolution.','USER','dbo','TABLE','CONSULT_ISSUE';
go

execute sp_addextendedproperty 'Description','CONSULTATION CROSS REFERENCE: this table is used to track relationships between consultations. Some consultations are associated with each other because of regulatory issues, others may be annual iterations of a master consultation and others may supplement or replace consultations.','USER','dbo','TABLE','CONSULT_XREF';
go

execute sp_addextendedproperty 'Description','LAND RIGHT CONTESTED: representation of information about contested land rights. Contestation may be internal (within a country) or external (between countries). Summary information about the cause and resolution of each contest may be tracked. In support of thebusiness requriements, land contests are associated only with land rights - they cannot be described as an independant entity. If business requirements exist for more detailed and complete information about land contests, additional modeling will be required.','USER','dbo','TABLE','CONTEST';
go

execute sp_addextendedproperty 'Description','CONTEST COMPONENT: the business objects that are associated with a contest.','USER','dbo','TABLE','CONTEST_COMPONENT';
go

execute sp_addextendedproperty 'Description','CONTEST PARTY: tracks the parties (Business Associates) who are involved with the contest. Parties may be litigators, defendants, plaintiffs, contractors or consultants, companies etc.','USER','dbo','TABLE','CONTEST_PARTY';
go

execute sp_addextendedproperty 'Description','CONTEST REMARK: narrative remarks about the contest.','USER','dbo','TABLE','CONTEST_REMARK';
go

execute sp_addextendedproperty 'Description','CONTRACT: a binding agreement between two or more parties for the express purpose of sharing risk with associated revenue and expenses in a exploitation or exploration undertaking or the joint building of a oil and gas production facility. An agreement for exploration or expoitation is always associated with substance(s) and zone(s) which have been granted by the mineral rights owner.','USER','dbo','TABLE','CONTRACT';
go

execute sp_addextendedproperty 'Description','CONTRACT COMPONENT: a table that associates a contract with land rights, seismic lines, projects, wells etc.','USER','dbo','TABLE','CONTRACT_COMPONENT';
go

execute sp_addextendedproperty 'Description','CONTRACT ACCOUNTING PROCEDURE: The accounting procedure defines those terms and conditions that must be adhered to by all business assoicates having a working interest in the lands convered by the contract. Accounting Procudures may be industry standard forms (e.g. PASC 1988 or COPAS 1986) or unique to a contract','USER','dbo','TABLE','CONT_ACCOUNT_PROC';
go

execute sp_addextendedproperty 'Description','CONTRACT ALIAS: a contract reference number related to another business associates internal contract reference.','USER','dbo','TABLE','CONT_ALIAS';
go

execute sp_addextendedproperty 'Description','CONTRACT ALLOWABLE EXPENSE: an amount or percent or description of a type of expense(s) that are agreed to and usually derived from the accounting procedure.','USER','dbo','TABLE','CONT_ALLOW_EXPENSE';
go

execute sp_addextendedproperty 'Description','CONTRACT AREA: Use this table to list the areas into which a contract falls. Note that the list may contain geographic overlaps and jurisdicational or regulatory overlaps.','USER','dbo','TABLE','CONT_AREA';
go

execute sp_addextendedproperty 'Description','CONTRACT BUSINESS ASSOCIATE: This table lists all the business associates involved in a contract and describes their role in contract creation, management or termination.','USER','dbo','TABLE','CONT_BA';
go

execute sp_addextendedproperty 'Description','CONTRACT BUSINESS ASSOCIATE SERVICE: A cross reference table allowing services provided by a business associate for the management or maintenance of the contract. This table should not be used to track partners.','USER','dbo','TABLE','CONT_BA_SERVICE';
go

execute sp_addextendedproperty 'Description','CONTRACT EXEMPTION: Describes which business associates are exempt from specific contractual obligations or other provisions.','USER','dbo','TABLE','CONT_EXEMPTION';
go

execute sp_addextendedproperty 'Description','CONTRACT EXTENSION: this table is used to describe extensions beyond the primary term that are granted to an contract. In some cases, these are granted through a application process (LR CONT APPLICATION) and in some cases they are granted automatically.','USER','dbo','TABLE','CONT_EXTENSION';
go

execute sp_addextendedproperty 'Description','CONTRACT JURISDICTION: a specified area determined by a governing agency, such as Alberta, Texas, Venezuela, municipalities or counties.','USER','dbo','TABLE','CONT_JURISDICTION';
go

execute sp_addextendedproperty 'Description','CONTRACT KEY WORD: a searchable key word found in the contract.','USER','dbo','TABLE','CONT_KEY_WORD';
go

execute sp_addextendedproperty 'Description','CONTRACT MARKETING ELECTION SUBSTANCE: the producing substance(s) to be marketed by the operator on behalf of the joint account.','USER','dbo','TABLE','CONT_MKTG_ELECT_SUBST';
go

execute sp_addextendedproperty 'Description','CONTRACT OPERATING PROCEDURE: This table outlines the operating procedure defined in the contract. Specific clauses may be stored in the CONT PROVISION TEXT table if desired.','USER','dbo','TABLE','CONT_OPER_PROC';
go

execute sp_addextendedproperty 'Description','CONTRACT PROVISION: an article or clause that introduces a condition or term which the fulfillment of an agreement depends (provides operational and/or earning requirements)','USER','dbo','TABLE','CONT_PROVISION';
go

execute sp_addextendedproperty 'Description','CONTRACT PROVISION TEXT: the actual text used in the distinct article in the formal document.','USER','dbo','TABLE','CONT_PROVISION_TEXT';
go

execute sp_addextendedproperty 'Description','CONTRACT PROVISION CROSS REFERENCE: tracks relationships between distinct articles in the formal document. (one proviso fulfillment relies on another proviso)','USER','dbo','TABLE','CONT_PROVISION_XREF';
go

execute sp_addextendedproperty 'Description','CONTRACT REMARK: a text description to record general comments on the contract tracking when remark was made, who is the author and the type of remark.','USER','dbo','TABLE','CONT_REMARK';
go

execute sp_addextendedproperty 'Description','CONTRACT STATUS: This table may be used to track the status of the contract as it changes over time. Various types of statuses may be captured, such as the operating status, financial status or legal status.','USER','dbo','TABLE','CONT_STATUS';
go

execute sp_addextendedproperty 'Description','CONTRACT TYPE: List of valid types for a specific contract, such as pooling agreement, joint venture, joint operating agreement, farm-out, etc.','USER','dbo','TABLE','CONT_TYPE';
go

execute sp_addextendedproperty 'Description','CONTRACT VOTING PROCEDURE: The table outlines the voting procedure set forth in a contract.','USER','dbo','TABLE','CONT_VOTING_PROC';
go

execute sp_addextendedproperty 'Description','CONTRACT CROSS REFERENCE: this table may be used to track relationships between contracts. Under certain conditions, a new contract may supercede, govern or replace another contract. Clauses or conditions in some contracts may clarify, define, elaborate on or specify the operation of clauses on another contract.','USER','dbo','TABLE','CONT_XREF';
go

execute sp_addextendedproperty 'Description','COORDINATE SYSTEM ALIAS: Allows users to refer to the coordinate systems by common use names, codes or acronyms.','USER','dbo','TABLE','CS_ALIAS';
go

execute sp_addextendedproperty 'Description','COORDINATE SYSTEM: a supertype representing all the types of coordinate sytems allowed in PPDM. Included Vertical, Geographic, Geocentric, Map Grid, Local Spatial and other coordinate systems. Tables are projected at the super type level. Details about the possilbe types of coordinate systems follow. GEOCENTRIC COORDINATE SYSTEM: Coordinate system based on the center of the earth where X, Y and Z are based on positions on the ellipsoid. GEOGRAPHIC COORDINATE SYSTEM: Latitude, longitude based horizontal coordinate system. LOCAL SPATIAL COORDINATE SYSTEM: Locally defined horizontal coordinate system, such as ATS 2.1. MAP GRID SYSTEM: Definition of a planar grid coordinate system for X,Y or Northing-Easting coordinate pairs. Also includes the projection type and the projection parameters used to project from the associated geodetic datum used to this g rid projection. OTHER COORDINATE SYSTEM: May include other non-spatial coordinate systems. VERTICAL DATUM: A reference surface used as the basis of elevation and depth measurements.','USER','dbo','TABLE','CS_COORDINATE_SYSTEM';
go

execute sp_addextendedproperty 'Description','COORD ACQUISITION: identifies an assembly of coordinate data that must be grouped together. It should be acquired together at the same time, from the same source, using the same method and at the same accuracy. May be used to indicate the level ofaccuracy of a set of coordinates.','USER','dbo','TABLE','CS_COORD_ACQUISITION';
go

execute sp_addextendedproperty 'Description','COORDINATE SYSTEM COORDINATE TRANSFORMATIONS: This table and its subordinates are used to capture details about how values in one coordinate system are transformed into another. Users should be aware that the contents of these tables are generally provided for reference only. They do not provide the full suite of functionality necessary to perform conversions. This function is best left to specialized software and service providers.','USER','dbo','TABLE','CS_COORD_TRANSFORM';
go

execute sp_addextendedproperty 'Description','COORDINATE SYSTEM TRANSFORMATION PARAMETERS: Parameter associated with a transformation between coordinate systems. This information is usually derived from the EPSG. Note that this model is not designed to support actual coordinate transformation, but toprovide a reference as to the conversions that are done.','USER','dbo','TABLE','CS_COORD_TRANS_PARM';
go

execute sp_addextendedproperty 'Description','COORDINATE TRANSFORMATION VALUE: the value assigned to a parameter for a coordinate transformation. Sample data is derived from the EPSG.','USER','dbo','TABLE','CS_COORD_TRANS_VALUE';
go

execute sp_addextendedproperty 'Description','ELLIPSOID: The ellipsoid of revolution, that describes the physical shape of the Earth. The ellipsoidal model is used by the geodetic datums and used to determine their mathematical coefficient sets.','USER','dbo','TABLE','CS_ELLIPSOID';
go

execute sp_addextendedproperty 'Description','GEODETIC DATUM: A coordinate system used to reference latitude and longitude values. Geodetic Datums are comprised of an Ellipsoid of Revolution, that is fixed in some manner to the physical Earth.','USER','dbo','TABLE','CS_GEODETIC_DATUM';
go

execute sp_addextendedproperty 'Description','PRIME MERIDIAN: The identification and definition of the starting longitude of a Geodetic coordinate system. Includes an offset longitude from the Greenwich Meridian.','USER','dbo','TABLE','CS_PRIME_MERIDIAN';
go

execute sp_addextendedproperty 'Description','PRINICPAL MERIDIAN CODE: A reference table identifying the valid principal meridians used for legal survey descriptions. This is the f irst meridian in the survey from which all other meridians are numbere d. For example Black Hi lls 1878, Boise 1867, Chicksaw 1833, ...','USER','dbo','TABLE','CS_PRINCIPAL_MERIDIAN';
go

execute sp_addextendedproperty 'Description','ECOZONE: A sedimentary rock unit or environment in which fossil deposition occurs. Often, these ecozones are marine or fresh water. Marine ecozones include the category of marine benthic zones, namely shelf, slope, and abyssal zones. The ecozone is defined by the organisms that are found in it.','USER','dbo','TABLE','ECOZONE';
go

execute sp_addextendedproperty 'Description','ECOZONE ALIAS: Alternate names, codes or identifiers for an ecozone. The preferred version of the name should also be loaded into this table. Use a trigger or procedure to update PREFERRED NAME in ECOZONE.','USER','dbo','TABLE','ECOZONE_ALIAS';
go

execute sp_addextendedproperty 'Description','ECOZONE SET HIERARCHY: Ecozones in a set are arranged in hierarchical order in this table, so that you can determine which ecozone is a parent or supertype of which.','USER','dbo','TABLE','ECOZONE_HIERARCHY';
go

execute sp_addextendedproperty 'Description','ECOZONE SET: this table is used to group ecozone definitions into sets used by an organization, for a project or over time. Associate each ecozone set with the ecozones it uses via the ECOZONE SET PART table.','USER','dbo','TABLE','ECOZONE_SET';
go

execute sp_addextendedproperty 'Description','ECOZONE SET PART: Ecozone sets are associated with the relevant ecozones in this table.','USER','dbo','TABLE','ECOZONE_SET_PART';
go

execute sp_addextendedproperty 'Description','ECOZONE CROSS REFERENCE: Ecozones may be related to each other in this table. For example, ecozones defined by one organization may overlap or correspond to other ecozone definitions.','USER','dbo','TABLE','ECOZONE_XREF';
go

execute sp_addextendedproperty 'Description','ENTITLEMENT: This table describes access and use priveledges or rights that are held by a person or organization.','USER','dbo','TABLE','ENTITLEMENT';
go

execute sp_addextendedproperty 'Description','ENTITLEMENT COMPONENT: The business objects defined in PPDM whose entitlement properties are managed by this entitlement description. May include wells, components of wells, seismic data, land rights etc.','USER','dbo','TABLE','ENT_COMPONENT';
go

execute sp_addextendedproperty 'Description','ENTITLEMENT SECURITY GROUP: This table is used to capture which security groups have access of varying types to the entitlement type.','USER','dbo','TABLE','ENT_GROUP';
go

execute sp_addextendedproperty 'Description','ENTITLEMENT SECURITY BUSINESS ASSOCIATE: The business associates (users, companies, organizations) who are part of an ENT_SECURITY_GROUP.','USER','dbo','TABLE','ENT_SECURITY_BA';
go

execute sp_addextendedproperty 'Description','ENTITLEMENT SECURITY: This table is used to capture information about various security groups defined by an organization. Each group can be granted different types of access to data based on their entitlements.','USER','dbo','TABLE','ENT_SECURITY_GROUP';
go

execute sp_addextendedproperty 'Description','ENTITLEMENT SECURITY GROUP CROSS REFERENCE: Use this table to track relationships between security groups, such as groups that overlap, replace, are part of etc.','USER','dbo','TABLE','ENT_SECURITY_GROUP_XREF';
go

execute sp_addextendedproperty 'Description','EQUIPMENT: Use this table to describe pieces of equipment that are real, as opposed to represented in a catalogue. May be any kind of equipment, such as trucks, rigs, computers, microscopes, gaugues etc.','USER','dbo','TABLE','EQUIPMENT';
go

execute sp_addextendedproperty 'Description','EQUIPMENT ALIAS: Equipment may have more than one name, code or identifier. Codes such as the UPC code or serial numbers may be stored in this table. Care should be taken to ensure that these listings are actually for the same equipment and not similar equipment. All possible names, codes and other identifiers can be stored here.','USER','dbo','TABLE','EQUIPMENT_ALIAS';
go

execute sp_addextendedproperty 'Description','EQUIPMENT BUSINESS ASSOCIATES: Use this table to track business associates who are involved with a piece of equipement, such as owners or people who lease or operate equipment.','USER','dbo','TABLE','EQUIPMENT_BA';
go

execute sp_addextendedproperty 'Description','EQUIPMENT COMPONENT: This table is used to capture the relationships between equipment and busines objects, such as wells, documents, seismic sets and contracts.','USER','dbo','TABLE','EQUIPMENT_COMPONENT';
go

execute sp_addextendedproperty 'Description','EQUIPMENT MAINTENANCE: Use this table to track scheduled and actual maintenance activities on a piece of equipment, such as cleaning, calibration, rebuilding etc. For more detail about maintenance and associated processes, please use the PROJECTS module. Financial information (costs) should be stored in the FINANCE tables.','USER','dbo','TABLE','EQUIPMENT_MAINTAIN';
go

execute sp_addextendedproperty 'Description','EQUIPMENT MAINTAIN STATUS: Use this table to keep track of the status of various phases of maintenance activities for a piece of equipment. For example, you may want to track when the activity was approved, when the equipment was sent out (or when a crew arrived), when the work was started, when it was tested, when it was finalized etc. This table can be used to help create metrics for equipment performance and maintenance schedules.','USER','dbo','TABLE','EQUIPMENT_MAINT_STATUS';
go

execute sp_addextendedproperty 'Description','EQUIPMENT MAINTENANCE TYPE: Use this table to track the various types of maintenace that may occur on equipment. This table has a two part primary key, so that each kind of equipment lists only the maintenance activities that are relevant for it.','USER','dbo','TABLE','EQUIPMENT_MAINT_TYPE';
go

execute sp_addextendedproperty 'Description','EQUIPMENT SPECIFICATIONS: Use this table to capture specifications for specific pieces of equipment. These specifications may vary among specific manufactured peices, such as calibration specifications.','USER','dbo','TABLE','EQUIPMENT_SPEC';
go

execute sp_addextendedproperty 'Description','EQUIPMENT SPECIFICATION SET: use this table to group together specifications into sets that are relevent for particular purposes, such as for a type of equipment.','USER','dbo','TABLE','EQUIPMENT_SPEC_SET';
go

execute sp_addextendedproperty 'Description','EQUIPMENT SPECIFICATION SET SPECIFICATIONS: Use this table to track which specifications are in a set, such as the set of specifications used to describe a vehicle, or a pipe.','USER','dbo','TABLE','EQUIPMENT_SPEC_SET_SPEC';
go

execute sp_addextendedproperty 'Description','EQUIPMENT STATUS: Tracks the status or condition of a peice of equipment over time. Can include information about commissioning and informtion about the condition of the equipment noted during inspections. May also be used to track DOWNTIMES using STATUS TYPE as downtime, and STATUS to track the kind of downtime.','USER','dbo','TABLE','EQUIPMENT_STATUS';
go

execute sp_addextendedproperty 'Description','EQUIPMENT USAGE STATISTICS: Use this table to track the usage of a specific piece of equipment, such as the distance driven by a truck, the total revolutions by a pump or the total used distance for a piece of coiled tubing. This information is used to assist with maintenace and replacment scheduling.','USER','dbo','TABLE','EQUIPMENT_USE_STAT';
go

execute sp_addextendedproperty 'Description','EQUIPMENT CROSS REFERENCE: This multi function table can keep track of peices of equipment that are part of the composition of a larger item, identify alternate or interchangable parts, indicate new parts that replace parts that are no longer manufactured etc. The reference table controls the contents, and should be carefuly managed.','USER','dbo','TABLE','EQUIPMENT_XREF';
go

execute sp_addextendedproperty 'Description','FACILITY: A collection of surface equipment and meters which facilitate the production, injection or disposition of products. This equipment supports any operation in the processing, development and transportation of products.','USER','dbo','TABLE','FACILITY';
go

execute sp_addextendedproperty 'Description','FACILITY ALIAS: Alias or alternate name for the facility.','USER','dbo','TABLE','FACILITY_ALIAS';
go

execute sp_addextendedproperty 'Description','FACILITY AREA: this table tracks the relationships between facilities and all areas that they intersect with. These areas may be formal geopolitical areas, business or regulatory areas, informal areas etc.','USER','dbo','TABLE','FACILITY_AREA';
go

execute sp_addextendedproperty 'Description','FACILITY SERVICE: this table may be used to track services that are provided for a facility, such as maintenance, inspections, supplies etc.','USER','dbo','TABLE','FACILITY_BA_SERVICE';
go

execute sp_addextendedproperty 'Description','FACILITY CLASS: Classifications for the facility, most notably classifications relating to the emmissions of hazardous products, are captured here. These classifications may change over time.','USER','dbo','TABLE','FACILITY_CLASS';
go

execute sp_addextendedproperty 'Description','FACILITY COMPONENT: This table is used to capture the relationships between facilities and busines objects, such as wells, equipment, documents, seismic sets and contracts.','USER','dbo','TABLE','FACILITY_COMPONENT';
go

execute sp_addextendedproperty 'Description','FACILITY DESCRIPTION: Use this vertical table to capture descriptive information about a facility, such as size or dimensions and other information that is not specifically supported in PPDM.','USER','dbo','TABLE','FACILITY_DESCRIPTION';
go

execute sp_addextendedproperty 'Description','FACILITY EQUIPMENT: Generally, a facility is usually considered to be an object that exists somewhere on the earth (or under or over). In most systems, these facilities are given identifiers that indicate a PLACE on a network, and not a specific piece of equipment. This table can be used to keep track of which equipment is occupying that facility place at a given time. A single facility can thus be associated with many physical pieces of equipment over its life span.','USER','dbo','TABLE','FACILITY_EQUIPMENT';
go

execute sp_addextendedproperty 'Description','FACILITY FIELD: cross reference table indicating which fields a facility is associated with. In some cases, facilities must be associated with a specific field, and in others a facility may serve more than one field.','USER','dbo','TABLE','FACILITY_FIELD';
go

execute sp_addextendedproperty 'Description','FACILITY LICENSE: Tacks authorizations of various types to conduct activities and operations related to a facility such as a pipeline, battery, pumping station etc. These authorizations may be called licenses, approvals, permits etc by various regulatory agencies.','USER','dbo','TABLE','FACILITY_LICENSE';
go

execute sp_addextendedproperty 'Description','FACILITY LICENSE NAME ALIAS: The alias table stores multiple alias names for a given license name.','USER','dbo','TABLE','FACILITY_LIC_ALIAS';
go

execute sp_addextendedproperty 'Description','FACILITY LICENSE AREA: This table provides a list of the areas into which a facility license falls.','USER','dbo','TABLE','FACILITY_LIC_AREA';
go

execute sp_addextendedproperty 'Description','FACILITY LICENSE CONDITION: lists the conditions under which the license or approval has been granted. May include payment of fees, development of agreements, performance of work etc. If desired, the project module may be used to track fulfillment of operational conditions. The obligations module is used to track payment of fees.','USER','dbo','TABLE','FACILITY_LIC_COND';
go

execute sp_addextendedproperty 'Description','FACILITY LICENSE REMARK: a text description to record general comments on the license tracking when remark was made, who is the author and the type of remark.','USER','dbo','TABLE','FACILITY_LIC_REMARK';
go

execute sp_addextendedproperty 'Description','FACILITY LICENSE STATUS: Tracks the status of a license throughout its lifetime. Various types of status may be included at the discretion of the implementor.','USER','dbo','TABLE','FACILITY_LIC_STATUS';
go

execute sp_addextendedproperty 'Description','FACILITY LICENSE TYPE: The type of facility license that is granted, such as processing, flaring, sales, venting etc. In some cases, all types may be combined into a single license and in others, multiple licenses may be granted.','USER','dbo','TABLE','FACILITY_LIC_TYPE';
go

execute sp_addextendedproperty 'Description','FACILITY LICENSE VIOLATION: Use this table to track incidents where the terms of a license have been violated (or perhaps are claimed to be violated). At this time the table is relatively simple in content.','USER','dbo','TABLE','FACILITY_LIC_VIOLATION';
go

execute sp_addextendedproperty 'Description','FACILITY MAINTAINANCE RECORD: General details about maintenace on the facility can be captured here.','USER','dbo','TABLE','FACILITY_MAINTAIN';
go

execute sp_addextendedproperty 'Description','FACILITY MAINTAIN STATUS: Use this table to keep track of the status of various phases of maintenance activities for a facility. For example, you may want to track when the activity was approved, when equipment was sent out (or when a crew arrived), whenthe work was started, when it was tested, when it was finalized etc. This table can be used to help create metrics for performance and maintenance schedules.','USER','dbo','TABLE','FACILITY_MAINT_STATUS';
go

execute sp_addextendedproperty 'Description','FACILITY RATE: this table may be used to capture which rate schedules apply to use of a faciility.','USER','dbo','TABLE','FACILITY_RATE';
go

execute sp_addextendedproperty 'Description','FACILITY RESTRICTION: Describes surface restrictions of various sorts, as defined and enforced by a jurisdictional body, such as a government or its agency. Detailed information about the surface restriction, such as its areal extent, restricted activities, contact information and descriptions can be found in associated tables.','USER','dbo','TABLE','FACILITY_RESTRICTION';
go

execute sp_addextendedproperty 'Description','FACILITY STATUS: Tracks the status of a facility throughout its lifetime. Various types of status may be included at the discretion of the implementor. May also include downtimes, using STATUS TYPE as downtime and the Status as the specific kind of downtimebeing tracked.','USER','dbo','TABLE','FACILITY_STATUS';
go

execute sp_addextendedproperty 'Description','FACILITY SUBSTANCE: This table may be used to track the ability of a facility to handle substances. Supporting information, such as capacity, is also provided.','USER','dbo','TABLE','FACILITY_SUBSTANCE';
go

execute sp_addextendedproperty 'Description','FACILITY VERSION: information about the facility from alternate sources. The Preferred version is stored in the FACILITY table.','USER','dbo','TABLE','FACILITY_VERSION';
go

execute sp_addextendedproperty 'Description','FACILITY CROSS REFERENCE: this table may be used to track the relationships between facilities. Use to track which tanks are in tank groups, which pipelines feed into which tanks, which processing units accept product from which tanks (or which storage tanks product can be sent to), which units have replaced worn out equipment etc.','USER','dbo','TABLE','FACILITY_XREF';
go

execute sp_addextendedproperty 'Description','FIELD: A geographical area defined for administrative and legal purposes. The field name refers to the surface area, although at times it may refer to both the surface and the underground productive zones. In the United States a field is often an area consisting of a single reservoir or multiple reservoirs all grouped on, or related to, the same individual geological structural feature and/or stratigraphic condition. Fields are usually defined at a province/state level but possibly are done at the district level.','USER','dbo','TABLE','FIELD';
go

execute sp_addextendedproperty 'Description','FIELD NAME ALIAS: The Field Name Alias table stores multiple field names assigned to a given field name. For example, the Hugoton Gas Field may have many versions of the name assigned by a regulatory body, such as, Hugoton G. Field etc. This table can translate all multiple field names into one common name.','USER','dbo','TABLE','FIELD_ALIAS';
go

execute sp_addextendedproperty 'Description','FIELD AREA: this table tracks the relationships between fields and all areas that they intersect with. These areas may be formal geopolitical areas, business or regulatory areas, informal areas etc.','USER','dbo','TABLE','FIELD_AREA';
go

execute sp_addextendedproperty 'Description','FIELD COMPONENT: This table is used to capture the relationships between fields and busines objects, such as wells, equipment, documents, seismic sets and contracts.','USER','dbo','TABLE','FIELD_COMPONENT';
go

execute sp_addextendedproperty 'Description','FIELD INSTRUMENT: This table may be used to track the relationship between fields and instruments. An instument may be regarded as a document that registers interest in something.','USER','dbo','TABLE','FIELD_INSTRUMENT';
go

execute sp_addextendedproperty 'Description','FIELD VERSION: a version of field information from a specific source. The Preferred version is inserted into the FIELD table.','USER','dbo','TABLE','FIELD_VERSION';
go

execute sp_addextendedproperty 'Description','FINANCE SUMMARY: Summary information about a financial reference, such as an AFE for an activity, such as seismic acquisition or processing. Could be a cost center or any other reference number.','USER','dbo','TABLE','FINANCE';
go

execute sp_addextendedproperty 'Description','FINANCE COMPONENT: The business objects in PPDM that are related to this financial summary. May be land rights, seismic acquisition or processing, well drilling, completions etc.','USER','dbo','TABLE','FIN_COMPONENT';
go

execute sp_addextendedproperty 'Description','FINANCE COST SUMMARY: this table is added to allow summaries of costs to be reported and allocated to various business objects. This table is not intended to be used as an accounting system, but may contain summaries derived from the accounting system.','USER','dbo','TABLE','FIN_COST_SUMMARY';
go

execute sp_addextendedproperty 'Description','AFE OR COST CENTER CROSS REFERENCE: this table is used to create relationships between AFE or cost center information. For example, the associated AFE may be a part of or a replacement for the parent.','USER','dbo','TABLE','FIN_XREF';
go

execute sp_addextendedproperty 'Description','FOSSIL: The remains or traces of animals or plants which have been preserved by natural causes in the earths crust exclusive of organisms which have been buried since the beginning of historic times.','USER','dbo','TABLE','FOSSIL';
go

execute sp_addextendedproperty 'Description','FOSSIL AGE: the age of a fossil Ages may be described in ordinal or chronological terms.','USER','dbo','TABLE','FOSSIL_AGE';
go

execute sp_addextendedproperty 'Description','FOSSIL ASSEMBLAGE: A grouping of fossils that are found to occur together in a sample. Can be autochthonous or allochthonous. Generally used interchangeably with the term biofacies. Assemblage zones are usually environmentally controlled and are useful only in local correlation. In a given sample, such as one collected over a thirty-foot interval, several biofacies may be grouped together. The sample is identified by the oldest biofacies, a characteristic fossil or an index fossil.','USER','dbo','TABLE','FOSSIL_ASSEMBLAGE';
go

execute sp_addextendedproperty 'Description','FOSSIL DESCRIPTION: Use this table to list morphological descriptive information about a fossil, such as length, height, width, number of spines, color.','USER','dbo','TABLE','FOSSIL_DESC';
go

execute sp_addextendedproperty 'Description','FOSSIL DOCUMENT: Lists the scientific literature in which a fossil has been described. These sources may be textbooks, journals or other publications.','USER','dbo','TABLE','FOSSIL_DOCUMENT';
go

execute sp_addextendedproperty 'Description','FOSSIL EQUIVALENCE: Use this table to indicate that two fossils, separately identified, are actually the same fossil.','USER','dbo','TABLE','FOSSIL_EQUIVALENCE';
go

execute sp_addextendedproperty 'Description','FOSSIL NAME SET: a set of fossils that has been grouped together for for a common purpose. For example, the MMS may define a standardized list of fossils that are used for all interpretations, a project may define a smaller set of fossils that will be used forthe purposes of the project or a company may define a standard set of preferred fossils.','USER','dbo','TABLE','FOSSIL_NAME_SET';
go

execute sp_addextendedproperty 'Description','FOSSIL NAME SET FOSSIL: the set of fossils that are used within a specified fossil name set.','USER','dbo','TABLE','FOSSIL_NAME_SET_FOSSIL';
go

execute sp_addextendedproperty 'Description','FOSSIL ALIAS: Alternate names, codes or identifiers for a fossil. The preferred version of the name should also be loaded into this table. Use this table to indicate the merging of two fossils into a single fossil, such as two fossils previously thought tobe seperate that are identified later to be the same fossil.','USER','dbo','TABLE','FOSSIL_TAXON_ALIAS';
go

execute sp_addextendedproperty 'Description','FOSSIL TAXON HIERARCHY: Use this table to describe the hierarchy between fossils. It is only necessary to populate these tables to the point that is useful for your implementation.','USER','dbo','TABLE','FOSSIL_TAXON_HIER';
go

execute sp_addextendedproperty 'Description','FOSSIL TAXONOMIC LEAF: This table is used to store fossil name information at various levels of the taxonomic hierarchy, such as group, species, genus etc.','USER','dbo','TABLE','FOSSIL_TAXON_LEAF';
go

execute sp_addextendedproperty 'Description','FOSSIL CROSS REFERENCE: This table is used to capture relationships between fossils that have been described or defined. Fossils with an affinity with each other may be described in this table. May also be used to indicate a single fossil that is later identified to be more than one fossil.','USER','dbo','TABLE','FOSSIL_XREF';
go

execute sp_addextendedproperty 'Description','HSE INCIDENT: Use this table to track incidents involving lost time or injuries to the crew.','USER','dbo','TABLE','HSE_INCIDENT';
go

execute sp_addextendedproperty 'Description','HSE INCIDENT BUSINESS ASSOCIATE: Use this table to track the involvement of crew members and crews or other involved parties (police, emergecy crews, inspectors etc) in incidents.','USER','dbo','TABLE','HSE_INCIDENT_BA';
go

execute sp_addextendedproperty 'Description','HSE INCIDENT CAUSE: Describes the causes of an incident, or a part of an incident (INCIDENT DETAIL). Can be negligence, equipment failure, weather, act of God, Act of Terrorism etc.','USER','dbo','TABLE','HSE_INCIDENT_CAUSE';
go

execute sp_addextendedproperty 'Description','HSE INCIDENT CLASS: Use this table to broadly classify the type of incident, usually in reporting terms. The detailed types of incidents are linked to the details, as various components of the incident may be classified differently.','USER','dbo','TABLE','HSE_INCIDENT_CLASS';
go

execute sp_addextendedproperty 'Description','INCIDENT CLASS ALIAS: Use this table to capture all the names, codes and identifiers assigned to an incident class.','USER','dbo','TABLE','HSE_INCIDENT_CLASS_ALIAS';
go

execute sp_addextendedproperty 'Description','HSE INCIDENT COMPONENT: This table is used to associate business objects, such as wells, seismic sets, facilities, building addresses etc to HSE incidents of any kind. An incident may involve one or more business objects. Use the TOTAL columns to calculate and store rolled up numbers for each crew or for each object, as your business rules dictate.','USER','dbo','TABLE','HSE_INCIDENT_COMPONENT';
go

execute sp_addextendedproperty 'Description','HSE INCIDENT DETAIL: Use this table to describe the things that happen as a result of the incident. Results can include fire, well collapse, evacuation etc. Use the WELL INCIDENT RESONSE table to track what you did in response to the result.','USER','dbo','TABLE','HSE_INCIDENT_DETAIL';
go

execute sp_addextendedproperty 'Description','HSE INCIDENT EQUIPMENT: Use this table to track the involvement of equipment and incidents.','USER','dbo','TABLE','HSE_INCIDENT_EQUIP';
go

execute sp_addextendedproperty 'Description','HSE INCIDENT EQUIVALENCE: Indicates equivalence types between various kinds of incidents, particularly when the reporting schedules for various organizations or jurisdications are being used.','USER','dbo','TABLE','HSE_INCIDENT_EQUIV';
go

execute sp_addextendedproperty 'Description','HSE INCIDENT INTERACTION: Use this table to create the complex relationships that describe an incident. For example, John was the crew chief who was driving the rig supply truck during the afternoon shift of September 20th. He was involved in a roll over accident in which his leg was injured. Details about each component of this report are stored in the associated table. The function of this table is to bring them together in a meaningful way.','USER','dbo','TABLE','HSE_INCIDENT_INTERACTION';
go

execute sp_addextendedproperty 'Description','HSE INCIDENT REMARK: Use this table to capture narrative or classified remarks relating to the HSE Incident. For example, remarks made by safety inspectors etc may be stored here. To capture things like incident reports, please use the RM Module.','USER','dbo','TABLE','HSE_INCIDENT_REMARK';
go

execute sp_addextendedproperty 'Description','HSE INCIDENT RESPONSE: Describes an action taken as a result of what happened at an incident. For example, an ambulance may be called, a reprimand entered into a file etc. For details about the work flow related to actions, we recommend you use the PROJECTS module.','USER','dbo','TABLE','HSE_INCIDENT_RESPONSE';
go

execute sp_addextendedproperty 'Description','HSE INCIDENT SET: Use this table to define sets of incidents that are recorded during well operations, usually according to reporting specifications laid out by regulation, best practice or corporate policy.','USER','dbo','TABLE','HSE_INCIDENT_SET';
go

execute sp_addextendedproperty 'Description','HSE INCIDENT SEVERITY: The severity of an actual incident, usually measured with a scale specific to the kind of incidents. For example, a vehicle incident could be an unsafe action, a near miss, or a hit. Incidents can be rated as minor, moderate or severe or according to any scale necessary.','USER','dbo','TABLE','HSE_INCIDENT_SEVERITY';
go

execute sp_addextendedproperty 'Description','HSE INCIDENT SEVERITY ALIAS: A table containing all of the names, codes and identifiers given to describe the severity of an incident detail type.','USER','dbo','TABLE','HSE_INCIDENT_SEV_ALIAS';
go

execute sp_addextendedproperty 'Description','HSE INCIDENT SUBSTANCE: Use this table to describe the various susbstances involved in an incident. Not all substances may be hydrocarbons, as they may also be fire retardants etc. Note that volumes should generally be captured in the PDEN tables, although columns have been created to allow you to capture amounts of NON HYDROCARBON substances (such as the amount of fire retardent).','USER','dbo','TABLE','HSE_INCIDENT_SUBSTANCE';
go

execute sp_addextendedproperty 'Description','HSE INCIDENT TYPE: The type of incident that has been reported, such as crash, automotive accident, fall from rig etc.','USER','dbo','TABLE','HSE_INCIDENT_TYPE';
go

execute sp_addextendedproperty 'Description','INCIDENT TYPE ALIAS: Use this table to capture all the names, codes and identifiers assigned to an incident type.','USER','dbo','TABLE','HSE_INCIDENT_TYPE_ALIAS';
go

execute sp_addextendedproperty 'Description','HSE INCIDENT WEATHER: Use this table to capture weather or oceanic conditions during an HSE Incident. During the incident, changes in the weather may be recorded as new records.','USER','dbo','TABLE','HSE_INCIDENT_WEATHER';
go

execute sp_addextendedproperty 'Description','INSTRUMENT: a legal document registered on the Mineral Title indicating an interest in the lands. e.g. mortgages, assignments, caveat, lein, Certificate of Title etc. One instrument may cover one or more leases or land rights etc. Discharge of instruments against specific land rights is captured in LAND_RIGHT_INSTRUMENT.','USER','dbo','TABLE','INSTRUMENT';
go

execute sp_addextendedproperty 'Description','INSTRUMENT AREA: this table tracks the relationships between instruments and all named areas that they intersect with. These areas may be formal geopolitical areas, business or regulatory areas, informal areas etc.','USER','dbo','TABLE','INSTRUMENT_AREA';
go

execute sp_addextendedproperty 'Description','INSRUMENT COMPONENT: This table is used to capture the relationships between instruments.','USER','dbo','TABLE','INSTRUMENT_COMPONENT';
go

execute sp_addextendedproperty 'Description','INSTRUMENT DETAIL: Use this table to capture specific information that is captured on an instrument, or about the instrument. Where specific columns exist for commonly used information, please use them. This table covers exceptions not handled by PPDM.','USER','dbo','TABLE','INSTRUMENT_DETAIL';
go

execute sp_addextendedproperty 'Description','INSTRUMENT CROSS REFERENCE: Allows relationships between instruments to be captured.','USER','dbo','TABLE','INSTRUMENT_XREF';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE INTEREST SET: An interest set is a bundle of interests that govern the operation of land rights, facilities, production, wells etc. Each interest set describes a single type of interest (working, royalty etc) and may desribe a partners key roles within the interest set (operatorship, address for service etc.). If the interest values or roles change, a new interest set must be created. The SEQ_NO is used to track versioning of the interest set over time.','USER','dbo','TABLE','INTEREST_SET';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE INTEREST SET COMPONENT: This table serves as a multi-node many to many breakout table allowing production, seismic, land rights, wells, facilities, fields etc to be associated with an interest set. Designates a unique set of interests for a specified asset(s) determined by the contract.','USER','dbo','TABLE','INT_SET_COMPONENT';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE INTEREST SET PARTNER: A partner in the Interest set.','USER','dbo','TABLE','INT_SET_PARTNER';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE INTEREST SET PARTNER CONTACT: A business associate who is a contact person for an interest set.','USER','dbo','TABLE','INT_SET_PARTNER_CONT';
go

execute sp_addextendedproperty 'Description','INTEREST SET STATUS: The status of a partnership, from a planning and approval perspective or an operational perspective. The status of the partnership from various perspectives (legal, finance, operations, land managers etc) may be tracked.','USER','dbo','TABLE','INT_SET_STATUS';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE INTEREST SET CROSS REFERENCE: This table allows relationships between interest sets to be tracked.','USER','dbo','TABLE','INT_SET_XREF';
go

execute sp_addextendedproperty 'Description','LAND AGREEMENT: a legal agreement between business associates for the completion of business, such as drilling, maintenance of surface or mineral rights, granted rights etc. LAND AGREEMENT is a valid type of LAND RIGHT.','USER','dbo','TABLE','LAND_AGREEMENT';
go

execute sp_addextendedproperty 'Description','LAND AGREEMENT PART: a portion of a valid LAND AGREEMENT that has been created for a specific reason, such as creation of a new partnership (interest set) etc.','USER','dbo','TABLE','LAND_AGREE_PART';
go

execute sp_addextendedproperty 'Description','LAND RIGHT ALIAS: An alternate name or file number by which the land right may be known.','USER','dbo','TABLE','LAND_ALIAS';
go

execute sp_addextendedproperty 'Description','LAND RIGHT AREA: this table may be used to track the relationship between a land right and various types of areas. Could be offshore areas, offshore intersect areas, areas of divestiture, areas of interest etc.','USER','dbo','TABLE','LAND_AREA';
go

execute sp_addextendedproperty 'Description','LAND RIGHT BUSINESS ASSOCIATE SERVICE: representation of the service provided for a land right by a business associate, such as mineral title search, brokerage service etc.','USER','dbo','TABLE','LAND_BA_SERVICE';
go

execute sp_addextendedproperty 'Description','LAND RIGHT OCCUPANT: This table is used to keep track of occupants on lands, usually surface lands such as grazing leases. At any given time, one or more business associates may be occupants, and the same business associate can be an occupant more than once in aspan of time.','USER','dbo','TABLE','LAND_OCCUPANT';
go

execute sp_addextendedproperty 'Description','LAND RIGHT REMARK: comments or text description for information pertaining to the Mineral Agreement. All remarks are qualified by a remarks type. Some remarks may be recommendations about the disposition of the land right - additional information about this type of remark is captured.','USER','dbo','TABLE','LAND_REMARK';
go

execute sp_addextendedproperty 'Description','LAND RIGHT: Describes the right to land, either the surface and/ or the mineral rights (track using LAND RIGHT CATEGORY). Land rights may be owned outright (Certificate of Title) or leased through an agreement. Agreements may be either primary (made directly with lessor) or secondary (made with other lessees). Relationships between land rights are found in LR XREF.','USER','dbo','TABLE','LAND_RIGHT';
go

execute sp_addextendedproperty 'Description','LAND RIGHT APPLICATION: this table associates a land right with applications that are associated with it, either through management of contractual obligations or through application for permission to conduct activities.','USER','dbo','TABLE','LAND_RIGHT_APPLIC';
go

execute sp_addextendedproperty 'Description','LAND RIGHT BA LICENSE: This table associates a land right (surface, mineral or both) with licenses that are associated with them.','USER','dbo','TABLE','LAND_RIGHT_BA_LIC';
go

execute sp_addextendedproperty 'Description','LAND RIGHT COMPONENT: This table is used to capture the relationships between land rights and busines objects, such as wells, equipment, documents, seismic sets and contracts.','USER','dbo','TABLE','LAND_RIGHT_COMPONENT';
go

execute sp_addextendedproperty 'Description','LAND RIGHT FACILITY: To track the relationship of a well to a Unit.','USER','dbo','TABLE','LAND_RIGHT_FACILITY';
go

execute sp_addextendedproperty 'Description','LAND RIGHT FIELD: a direct relationship specifiying the fields that are associated with a Land Right (e.g. Bellshill, Ricinos).','USER','dbo','TABLE','LAND_RIGHT_FIELD';
go

execute sp_addextendedproperty 'Description','LAND RIGHT INSTRUMENT CROSS REFERENCE: Cross reference between the land right and the relevant instrument, in the case where there are many instruments for a land right, and each instrument may reference more than one land right. Occurs in the USA, for CASE registrations.','USER','dbo','TABLE','LAND_RIGHT_INSTRUMENT';
go

execute sp_addextendedproperty 'Description','LAND RIGHT POOL: This cross reference table allows relationships between pools and the land rights they are associated with to be maintained explicitly.','USER','dbo','TABLE','LAND_RIGHT_POOL';
go

execute sp_addextendedproperty 'Description','LAND RIGHT RESTRICTION: a cross reference table that identifies what restrictions are applied to a specific land right.','USER','dbo','TABLE','LAND_RIGHT_REST';
go

execute sp_addextendedproperty 'Description','LAND RIGHT RESTRICTION REMARK: narrative remarks about a land restriction as it is applied to a land right.','USER','dbo','TABLE','LAND_RIGHT_REST_REM';
go

execute sp_addextendedproperty 'Description','LAND RIGHT WELL: Tracks which wells are located on or producing from specific land rights. The entity PROD STRING is used as an alternate foreign key, so that specific well strings may be identified when necessary.','USER','dbo','TABLE','LAND_RIGHT_WELL';
go

execute sp_addextendedproperty 'Description','LAND RIGHT WELL SUBSTANCE: Describes the percent production spacing unit for a particular substance which is occupied by the land right. Oil and gas are captured in LAND RIGHT WELL. All other substances are captured here as required.','USER','dbo','TABLE','LAND_RIGHT_WELL_SUBST';
go

execute sp_addextendedproperty 'Description','LAND SALE: A public offering of lands for lease. Government lessors usually offer leases through land sales so that resources can be developed. A land sale may be composed of one or many land parcels.','USER','dbo','TABLE','LAND_SALE';
go

execute sp_addextendedproperty 'Description','LAND RIGHT SALE BUSINESS ASSOCIATE SERVICE: representation of the service provided for a land sale or bidding rounc by a business associate, such as mineral title search, brokerage service etc.','USER','dbo','TABLE','LAND_SALE_BA_SERVICE';
go

execute sp_addextendedproperty 'Description','LAND SALE OFFERING BID: the entity is used when bids placed on land offerings are complex. In some areas, bids may be placed contingent on success of other bids, sliding bids / offers may be applied or grouped bids may be offered. This entity allows trackingof the structure of a complex bid.','USER','dbo','TABLE','LAND_SALE_BID';
go

execute sp_addextendedproperty 'Description','LAND SALE BID SET: the entity is used when bids placed on land offerings are grouped according to some requirement. A company may group bids to support a specific play or project at a company, or financially, or organizationally. This table tracks the setor group of bids. Use LAND SALE BID SET BID to relate the set to individual bids.','USER','dbo','TABLE','LAND_SALE_BID_SET';
go

execute sp_addextendedproperty 'Description','LAND SALE OFFERING BID SET: the entity is used when bids placed on land offerings are grouped according to some requirement. A company may group bids to support a specific play or project at a company, or financially, or organizationally.','USER','dbo','TABLE','LAND_SALE_BID_SET_BID';
go

execute sp_addextendedproperty 'Description','LAND SALE FEE: This table is used to associate the appropriate fee schedule(s) with each land sale. While many regulatory bodies have only one fee schedule associated with a land sale, many others have multiple fee schedules.','USER','dbo','TABLE','LAND_SALE_FEE';
go

execute sp_addextendedproperty 'Description','LAND SALE OFFERING: describes a parcel of land offered in public land sale. Many such offerings may be included in a single land sale.','USER','dbo','TABLE','LAND_SALE_OFFERING';
go

execute sp_addextendedproperty 'Description','LAND SALE OFFERING AREA: captures the relationships between areas and land sale offerings. Opportunities to indicate the gross and net overlapping sizes are also provided. Note that these overlaps may be determined legally rather than spatially.','USER','dbo','TABLE','LAND_SALE_OFFERING_AREA';
go

execute sp_addextendedproperty 'Description','LAND REQUEST: tracks requests by business associates to a lessor to have a particular land parcel or holding posted for public sale of the granted rights.','USER','dbo','TABLE','LAND_SALE_REQUEST';
go

execute sp_addextendedproperty 'Description','LAND SALE RESTRICTION: A cross-reference table to handle the relationship of specific surface restriction(s) to specified land sale offering(s) at a specified Land Sale.','USER','dbo','TABLE','LAND_SALE_RESTRICTION';
go

execute sp_addextendedproperty 'Description','LAND SALE RESTRICTION REMARK: A text description to provide additional information about a surface restriction which could impact on operations. Remarks may be used to clarify the times or seasons that the restriciton is active to to describe the administrative requirements for the restriction.','USER','dbo','TABLE','LAND_SALE_REST_REMARK';
go

execute sp_addextendedproperty 'Description','LAND WORK BID: describes the work that an organization commits to in return for land rights. Use this table to describe the number of wells to be drilled, km of seismic to be shot or exploration value to be expended over the term of the work obligation.','USER','dbo','TABLE','LAND_SALE_WORK_BID';
go

execute sp_addextendedproperty 'Description','LAND RIGHT NET SIZE: representation of the relative amount of land held by a particular partner. In fact, the partner owns a percentage of the whole land, but for reporting purposes, this percentage is often represented as the number of acres held. For example, if a partner owns 50% of 160 acres (the gross acreage), his net acreage is reported as 80 acres.','USER','dbo','TABLE','LAND_SIZE';
go

execute sp_addextendedproperty 'Description','LAND RIGHT STATUS: describes the status of the land right. Status may include terminated, expired, active etc.','USER','dbo','TABLE','LAND_STATUS';
go

execute sp_addextendedproperty 'Description','LAND RIGHT TERMINATION: Verifies that all obligations have been met, and all legal or agreed upon requirements to terminate an agreement have been fulfilled.','USER','dbo','TABLE','LAND_TERMINATION';
go

execute sp_addextendedproperty 'Description','LAND TITLE: Registered interest in land which is wholly owned by the person(s) named on the Certificate of Title. Land titles may refer to the SURFACE, MINERAL RIGHTS or both.','USER','dbo','TABLE','LAND_TITLE';
go

execute sp_addextendedproperty 'Description','LAND RIGHT TRACT UNIT FACTOR: Use this table to capture the tract factors applied to the land right portion contributed to the unitization. Tract factors are usually assigned by substance.','USER','dbo','TABLE','LAND_TRACT_FACTOR';
go

execute sp_addextendedproperty 'Description','LAND UNIT: the area incorporated to consolidate numerous tracts to operate them as a single unit for allocating revenues, costs and expenses. A combination of leases, usually contiguous, involving potential or producing mineral properties, for the purpose of efficient or economic operation.','USER','dbo','TABLE','LAND_UNIT';
go

execute sp_addextendedproperty 'Description','LAND UNIT TRACT: A land right that has been assigned to a LAND UNIT, and that has been assigned a relative value for the unit agreement, called a tract factor. Usually an area of common ownership with respect to interest in minerals.','USER','dbo','TABLE','LAND_UNIT_TRACT';
go

execute sp_addextendedproperty 'Description','LAND RIGHT CROSS REFERENCE: represents the relationships between different types of land right. For example, the agreements which pertain to specific mineral agreements, or secondary agreements associated with the primary agreement may be tracked. May also be used to track relationships between various types of surface rights such as road agreements and easements.','USER','dbo','TABLE','LAND_XREF';
go

execute sp_addextendedproperty 'Description','LEGAL CARTER LOCATION: The Carter Location table describes the location of a cartographic object in reference to the Carter Grid Survey System which is a township, range and section system using latitude and longitude lines for subdivision boundaries. This land survey system is primarily used in the United States for the states of Kentucky and Tennessee.','USER','dbo','TABLE','LEGAL_CARTER_LOC';
go

execute sp_addextendedproperty 'Description','LEGAL CONGRESSIONAL LOCATION: The Congress Location table describes the location of an object in reference to the Congressional Land Survey System which forms a grid system of townships, ranges and sections. This rectilinear system is also known as Congressional, Jeffersonian, Civil or Dominion Surveys. It is the basic survey system used in the U.S. for surveying civil boundaries below the county level.','USER','dbo','TABLE','LEGAL_CONGRESS_LOC';
go

execute sp_addextendedproperty 'Description','LEGAL DOMINION LAND SURVEY LOCATION: The DLS table describes the location of an objectbased on the Canadian Dominion Land Survey (DLS). This survey system is used in the Canadian provinces of Manitoba, Saskatchewan, Alberta and in the Peac e River Block of British Columbia .','USER','dbo','TABLE','LEGAL_DLS_LOC';
go

execute sp_addextendedproperty 'Description','LEGAL FEDERAL PERMIT SYSTEM LOCATION: The description of locations in the Federal Permit System used in Canada. This system is used in all Canadian Federal Lands in both the offshore and the northern territories.','USER','dbo','TABLE','LEGAL_FPS_LOC';
go

execute sp_addextendedproperty 'Description','GEODETIC LOCATION: Describes a location in the virtual Geodetic System. It incorporates latitude and longitude values directly, rather than referencing them to a theoretical system. It may be used where the civil land survey system is not suited to the petroleum industry. For example, it is used in all the Canadian provinces east of Manitoba.','USER','dbo','TABLE','LEGAL_GEODETIC_LOC';
go

execute sp_addextendedproperty 'Description','SPATIAL AREA: Use this table to relate land parcel areas in the SP_% tables to AREA table.','USER','dbo','TABLE','LEGAL_LOC_AREA';
go

execute sp_addextendedproperty 'Description','LEGAL LOCATION REMARK: Contains information about the narrative description of the location. Typically, this data is miscellaneous comments about the location which does not fit into fixed fields.','USER','dbo','TABLE','LEGAL_LOC_REMARK';
go

execute sp_addextendedproperty 'Description','LEGAL NORTHEAST US LOCATION: The North East Location table contains location information pertaining to States located in the North East region of the United States.','USER','dbo','TABLE','LEGAL_NE_LOC';
go

execute sp_addextendedproperty 'Description','LEGAL NORTH SEA LOCATION: This table describes the European locations and is primarily used to store the coordinates of an Object surveyed within the North Sea system of offshore areas, blocks, and block subdivisions.','USER','dbo','TABLE','LEGAL_NORTH_SEA_LOC';
go

execute sp_addextendedproperty 'Description','LEGAL NATIONAL TOPOGRAPHIC SERIES LOCATION: Describes a location in reference to the National Topographic Series survey system used in British Columbia, Canada.','USER','dbo','TABLE','LEGAL_NTS_LOC';
go

execute sp_addextendedproperty 'Description','LEGAL OFFSHORE LOCATION: This table locates an object within a grid of blocks covering U.S. Federal and State waters. The offshore location table includes the Gulf states and Outer Continental Shelf area.','USER','dbo','TABLE','LEGAL_OFFSHORE_LOC';
go

execute sp_addextendedproperty 'Description','LEGAL OHIO LOCATION: The Ohio Location table describes the location of an object within the state of Ohio. This land survey scheme is patterned for identifying Ohio wellbore locations.','USER','dbo','TABLE','LEGAL_OHIO_LOC';
go

execute sp_addextendedproperty 'Description','LEGAL TEXAS LOCATION: Describes the location of an object in reference to a Texas land survey.','USER','dbo','TABLE','LEGAL_TEXAS_LOC';
go

execute sp_addextendedproperty 'Description','LITHOLOGIC DEPOSITIONAL ENVIRONMENT INTERVAL- an interpreted depositional environment over a specified interval.','USER','dbo','TABLE','LITH_DEP_ENV_INT';
go

execute sp_addextendedproperty 'Description','LITHOLOGIC DIAGENESIS - a description of the post depositional alteration.','USER','dbo','TABLE','LITH_DIAGENESIS';
go

execute sp_addextendedproperty 'Description','LITHOLOGIC GRAIN SIZE - a description of grain or crystal sizes of rock components.','USER','dbo','TABLE','LITH_GRAIN_SIZE';
go

execute sp_addextendedproperty 'Description','LITHOLOGIC INTERVAL - a depth interval of a descriptive record of lithology.','USER','dbo','TABLE','LITH_INTERVAL';
go

execute sp_addextendedproperty 'Description','LITHOLOGIC LOG - a descriptive record of lithology.','USER','dbo','TABLE','LITH_LOG';
go

execute sp_addextendedproperty 'Description','LITHOLOGY LOG BUSINESS ASSOCIATE SERVICE: A cross reference table allowing services provided by a business associate for the creation, analysis and mangement of logs.','USER','dbo','TABLE','LITH_LOG_BA_SERVICE';
go

execute sp_addextendedproperty 'Description','LITHOLOGY LOG COMPONENT: This table is used to capture the relationships between lithological logs and busines objects, such as wells, equipment, documents, seismic sets and contracts.','USER','dbo','TABLE','LITH_LOG_COMPONENT';
go

execute sp_addextendedproperty 'Description','LITHOLOGIC LOG REMARK: Narrative remarks about a lithologic log.','USER','dbo','TABLE','LITH_LOG_REMARK';
go

execute sp_addextendedproperty 'Description','LITHOLOGIC MEASURED SECTION: an aggegate description record of the stratigraphic thickness and lithology. STRATIGRAPHIC FIELD STATION: any location where geological studies or analysis or observations are carried out, such as at a measured section, outcrop etc.','USER','dbo','TABLE','LITH_MEASURED_SEC';
go

execute sp_addextendedproperty 'Description','LITHOLOGIC POROSITY: the observed porosity of lithologic sample','USER','dbo','TABLE','LITH_POROSITY';
go

execute sp_addextendedproperty 'Description','LITHOLOGIC MAJOR ROCK TYPE COMPONENT: description of major or minor rock component. Can include fossils.','USER','dbo','TABLE','LITH_ROCKPART';
go

execute sp_addextendedproperty 'Description','LITHOLOGIC ROCKPART COLOR: This table records descriptions of the color of the principle rock type component or accessory. Fossil or mineral constituents have certain color characteristics and this table defines the basic color, weathering, intensity and color distribution.','USER','dbo','TABLE','LITH_ROCKPART_COLOR';
go

execute sp_addextendedproperty 'Description','LITHOLOGIC ROCKPART GRAIN SIZE: Contains information about measured sizes in the rock component. The information contained in this table includes the actual size of the grain the the type of the scaling system source used to determine the grain size (e.g. Wentworth scale)','USER','dbo','TABLE','LITH_ROCKPART_GRAIN_SIZE';
go

execute sp_addextendedproperty 'Description','LITHOLOGIC ROCK COLOR: a description of the color of the principle rock type.','USER','dbo','TABLE','LITH_ROCK_COLOR';
go

execute sp_addextendedproperty 'Description','LITHOLOGIC ROCK STRUCTURE: Contains information about the physical structures within a major rock thpe. In sedimentary rocks, a structure is defined as a feature that was formed during the deposition of the sediments. Examples include bedding and liminations, cross-stratification, muc cracks etc. Sedimentary structures can be used to interpret the depositional environment of the sediments. This table can also be used for non-sedimentary (igneous and metamorphic) rocks to describe structures, such as contorted bedding or fault zones.','USER','dbo','TABLE','LITH_ROCK_STRUCTURE';
go

execute sp_addextendedproperty 'Description','LITHOLOGIC ROCK TYPE- a description of principle rock type comprising an interval.','USER','dbo','TABLE','LITH_ROCK_TYPE';
go

execute sp_addextendedproperty 'Description','LITHOLOGIC STRUCTURE: the physical structure within a major rock type or sub interval','USER','dbo','TABLE','LITH_STRUCTURE';
go

execute sp_addextendedproperty 'Description','NOTIFICATION: Use this table to capture notifications, such as those made for a land right or a contract (or in some cases, both). You can capture the type of notification that was made, the obligation that this notification satifies and whether it was served. Additional detail about serving the notification to the appropriate business associates may be found in the table NOTIF BA.','USER','dbo','TABLE','NOTIFICATION';
go

execute sp_addextendedproperty 'Description','NOTIFICATION COMPONENT: This table is used to capture the relationships between notifications and busines objects, such as wells, equipment, documents, seismic sets and contracts.','USER','dbo','TABLE','NOTIFICATION_COMPONENT';
go

execute sp_addextendedproperty 'Description','NOTIFICATION PARTY: this table may be used to identify all parties who send or receive notifications. It tracks when each notification was sent and received by each party.','USER','dbo','TABLE','NOTIF_BA';
go

execute sp_addextendedproperty 'Description','OBLIGATION: a condition of commitment on the mineral agreement which must be fulfilled by the lessee. There are many types of obligations and the fulfillment of an obligation may require a change to the agreement. e.g. offset obligation, work commitment, section 96 etc. Obligations may be financial (rental payments) or non financial (drilling, reporting etc.) Three sub types of obligation have been defined: rental, lease and royalty.','USER','dbo','TABLE','OBLIGATION';
go

execute sp_addextendedproperty 'Description','OBLIGATION COMPONENT: Use this table to capture the relationships between the obligation and other business objects, such as land rights, contracts, facilities or wells. Each row in the table should capture one and only one relationship. Use as many rowsas needed to capture all relevant relationships.','USER','dbo','TABLE','OBLIGATION_COMPONENT';
go

execute sp_addextendedproperty 'Description','OBLIGATION ALLOWABLE DEDUCTION: deductions that can be made to the gross obligation, based on contractual terms, statutory regulations or legislation or other terms.','USER','dbo','TABLE','OBLIG_ALLOW_DEDUCTION';
go

execute sp_addextendedproperty 'Description','OBLIGATION BUSINESS ASSOCIATE: indicates associations between obligations and business associates, generally for the fulfillement of work related obligations.','USER','dbo','TABLE','OBLIG_BA_SERVICE';
go

execute sp_addextendedproperty 'Description','OBLIGATION CALCULATION: This table is used to capture details about how an obligation is calculated. A vertical table structure allows great flexibility in the types of calculations that are supported.','USER','dbo','TABLE','OBLIG_CALC';
go

execute sp_addextendedproperty 'Description','OBLIGATION DEDUCTION: describes a deduction made from an obligation such as a land right royalty payment. This table will be defined in more detail in future modeling cycles and is subject to change.','USER','dbo','TABLE','OBLIG_DEDUCTION';
go

execute sp_addextendedproperty 'Description','OBLIGATION DEDUCTION CALCULATION: This table is used to calculate valid deductions from the payment of an obligation. The vertical nature of the table provides a great deal of flexibility in the type of calculations that cam be supported.','USER','dbo','TABLE','OBLIG_DEDUCT_CALC';
go

execute sp_addextendedproperty 'Description','OBLIGATION PAYMENT: A land right obligation payment is the rental or other payment amount paid by the maintainor to the lessor on behalf of the lessees over the lifetime of the land right.','USER','dbo','TABLE','OBLIG_PAYMENT';
go

execute sp_addextendedproperty 'Description','OBLGATION PAYMENT INSTRUCTIONS: The payment instructions provide directions for direct transfer of funds, banking instructions, and other payment information necessary for the automted generation of payments.','USER','dbo','TABLE','OBLIG_PAYMENT_INSTR';
go

execute sp_addextendedproperty 'Description','OBLIGATION PAYMENT RATE: A rate set by regulation and law for Crown or Federal Mineral rights or a negotiated rate on Freehold, Fee Lands, Pan Canadian or First Nations Mineral Rights.','USER','dbo','TABLE','OBLIG_PAYMENT_RATE';
go

execute sp_addextendedproperty 'Description','OBLIGATION PAYMENT DETAIL: breakdown of the detail of how each payment was made. For example, a rental payment may be broken down into multiple payments, one to each lessor partner. In this case, the percentage of the total payment made out to each partner is tracked.','USER','dbo','TABLE','OBLIG_PAY_DETAIL';
go

execute sp_addextendedproperty 'Description','OBLIGATION REMARK: General narrative remarks about the obligation may be stored in this table.','USER','dbo','TABLE','OBLIG_REMARK';
go

execute sp_addextendedproperty 'Description','OBLIGATION SUBSTANCE: This table is used to capture the substances that are covered by an obligation. Support for take in kind agreements are supported with this information.','USER','dbo','TABLE','OBLIG_SUBSTANCE';
go

execute sp_addextendedproperty 'Description','OBLIGATION TYPE: The type or classification of an obligation. A vertical table is provided to support business requirements that obligations may be described as having many types.','USER','dbo','TABLE','OBLIG_TYPE';
go

execute sp_addextendedproperty 'Description','OBLIGATION CROSS REFERENCE: This table is added to allow obligations to be associated with related obligations. For example, a rental obligation stipulated for a lease might be broken down into obligation components associated with subordinate lease segments (granted rights or tracts) to allow the rental cost to be shared proportionaly among partners. Also will allow tracking of a set of obligation components which are part of a larger obligation, esp for work related.','USER','dbo','TABLE','OBLIG_XREF';
go

execute sp_addextendedproperty 'Description','PALEO ABUNDANCE QUALIFIER: a qualifier that is added to an identification of the abundance of a fossil or its interpretation.','USER','dbo','TABLE','PALEO_ABUND_QUALIFIER';
go

execute sp_addextendedproperty 'Description','PALEO ABUNDANCE SCHEME: this table contains the denormalized information about schemes that are used to measure relative abundance and the scores that are associated with the schemes.','USER','dbo','TABLE','PALEO_ABUND_SCHEME';
go

execute sp_addextendedproperty 'Description','PALEONTOLOGICAL CLIMATE: The average state or typical conditions of climate during some past geological period.','USER','dbo','TABLE','PALEO_CLIMATE';
go

execute sp_addextendedproperty 'Description','PALEO CONFIDENCE: This table indicates the certainty in a paleontological interpretation, such as the identification of a fossil, ecozone definition etc.','USER','dbo','TABLE','PALEO_CONFIDENCE';
go

execute sp_addextendedproperty 'Description','PALEO FOSSIL PALEO INDICATOR: A set of indicator types typically generated during fossil analysis and interpretation. Can include youngest, oldest, deepest, reworked, out of place, etc.','USER','dbo','TABLE','PALEO_FOSSIL_IND';
go

execute sp_addextendedproperty 'Description','PALEO FOSSIL INTERPRETATION: This table lists the fossils that were used to support the interpretation in PALEO INTERP.','USER','dbo','TABLE','PALEO_FOSSIL_INTERP';
go

execute sp_addextendedproperty 'Description','PALEO FOSSIL LIST: this table provides a list of all the fossils identified during analysis, grouped using FOSSIL DETAIL ID based on a business grouping, such as a sample.','USER','dbo','TABLE','PALEO_FOSSIL_LIST';
go

execute sp_addextendedproperty 'Description','PALEO FOSSIL LIST OBSERVATION: this table provides a list of all the observataions about fossils identified during analysis, such as first, last, youngest, oldest, deepest etc.','USER','dbo','TABLE','PALEO_FOSSIL_OBS';
go

execute sp_addextendedproperty 'Description','PALEONTOLOGICAL INTERPRETATION: this table summarizes the interpretations made for the report. Each row is used to identify the assemblage, ecozone, lithology, relative lithostratigraphic or chronostratigraphic units interpreted to be found.','USER','dbo','TABLE','PALEO_INTERP';
go

execute sp_addextendedproperty 'Description','PALEO OBSERVATION QUALIFIER: a qualifier that is added to an identification of a fossil or its interpretation. Can include values such as AT, IN etc.','USER','dbo','TABLE','PALEO_OBS_QUALIFIER';
go

execute sp_addextendedproperty 'Description','PALEO SUMMARY: Header information for the Paleontological study that was done.','USER','dbo','TABLE','PALEO_SUMMARY';
go

execute sp_addextendedproperty 'Description','PALEO SUMMARY AUTHOR: This table tracks the authors of a paleontological report. May include corporate, technical, scientific authors.','USER','dbo','TABLE','PALEO_SUM_AUTHOR';
go

execute sp_addextendedproperty 'Description','PALEO SUMMARY COMPONENT: Lists all the components that are associated with the report, such as lithologic samples, items stored in the records management module etc.','USER','dbo','TABLE','PALEO_SUM_COMP';
go

execute sp_addextendedproperty 'Description','PALEO SUMMARY INTERVAL: An interval defined for the summary, usually when for a survey done in a well bore.','USER','dbo','TABLE','PALEO_SUM_INTERVAL';
go

execute sp_addextendedproperty 'Description','PALEO SUMMARY SAMPLE: Used to associate information in the paleo summary with the lithologic samples used.','USER','dbo','TABLE','PALEO_SUM_SAMPLE';
go

execute sp_addextendedproperty 'Description','PALEO SUMMARY CROSS REFERENCE: this table is used to capture relationships between reports, such as regional reports that are compiled from many well or measured section based reports.','USER','dbo','TABLE','PALEO_SUM_XREF';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY: This table represents any entity for which product ion could be reported against. This entity could be physical installations such as a production well string, a spatial construct such as lease or reservoir or it could be an organizational concept such as business unit.','USER','dbo','TABLE','PDEN';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY ALLOCATION FACTOR: This table represents a factor used in calculations to allocate production from one entity to another .','USER','dbo','TABLE','PDEN_ALLOC_FACTOR';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY AS AREA: This table facilitates the representation of any type of area such as a county as a production reporting entity. It allows a different set of identifiers and relationships for production reporting purposes. For instance, an organization may have an internal identifier for a county that is different than what is used by the organization or its business partners for reporting purposes.','USER','dbo','TABLE','PDEN_AREA';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY AS BUSINESS ASSOCIATE: This table facilitates the representation of a business associate as a production reporting entity. It allows a different set of identifiers and relationships for production reporting purposes. For instance, anorganization may have an internal identifier for a business associate that is different than what is used by the organization or its business partners for reporting purposes.','USER','dbo','TABLE','PDEN_BUSINESS_ASSOC';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY COMPONENT: This table is used to capture the relationships between production entities and busines objects, such as wells, equipment, documents, seismic sets and contracts.','USER','dbo','TABLE','PDEN_COMPONENT';
go

execute sp_addextendedproperty 'Description','PDEN DECLINE FORECAST CASE: Summarizes the parameters making up one or more decline segments used to forecast future production.','USER','dbo','TABLE','PDEN_DECLINE_CASE';
go

execute sp_addextendedproperty 'Description','PDEN DECLINE FORECAST CASE: Summarizes the parameters making up one or more decline segments.','USER','dbo','TABLE','PDEN_DECLINE_CONDITION';
go

execute sp_addextendedproperty 'Description','PDEN DECLINE SEGMENT: Contains the parameters used to forecast future production using standard decline curve analysis.','USER','dbo','TABLE','PDEN_DECLINE_SEGMENT';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY AS FACILITY: This table facilitates the representation of a facility as a production reporting entity. It allows a different set of identifiers and relationships for production reporting purposes. For instance, an organization may have an internal identifier for a facility that is different than what is used by the organization or its business partners for reporting purposes.','USER','dbo','TABLE','PDEN_FACILITY';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY AS FIELD: This table facilitates the representation of a field as a production reporting entity. It allows a different set of identifiers and relationships for production reporting purposes. For instance, an organization may have an internal identifier for a field that is different than what is used by the organization or its business partners for reporting purposes.','USER','dbo','TABLE','PDEN_FIELD';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY FLOW MEASUREMENT: Flow measurement readings associated with a production reporting entity. Records data from field automation readings including hourly or daily fluid volume with associated wellhead and line pressures and flow rate.','USER','dbo','TABLE','PDEN_FLOW_MEASUREMENT';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY IN COUNTY: Identifies the counties that an entity reporting production covers in full or in part.','USER','dbo','TABLE','PDEN_IN_AREA';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY AS LAND RIGHT: This table facilitates the representation of a land right as a production reporting entity. It allows a different set of identifiers and relationships for production reporting purposes. For instance, an organization may have an internal identifier for a land right that is different than what is used by the organization or its business partners for reporting purposes.','USER','dbo','TABLE','PDEN_LAND_RIGHT';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY AS LEASE OR UNIT: This table facilitates the representation of a lease or unit as a production reporting entity. It allows a different set of identifiers and relationships for production reporting purposes. For instance, an organization may have an internal identifier for a lease or unit that is different than what is used by the organization or its business partners for reporting purposes. Note that lease and unit are often treated synonymously for production reporting purposes and it is sometimes not possible to distinguish between them on the basis of production reports. For this reason they are treated as a single entity. The entity actually represents an alias used for production reporting purposes.','USER','dbo','TABLE','PDEN_LEASE_UNIT';
go

execute sp_addextendedproperty 'Description','PDEN MATERIAL BALANCE: Contains the parameters used to establish the original gas in place and recoverable gas in place using P/Z analysis.','USER','dbo','TABLE','PDEN_MATERIAL_BAL';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY OPERATOR HISTORY: Contains an historical account of the operators responsible for a production entity.','USER','dbo','TABLE','PDEN_OPER_HIST';
go

execute sp_addextendedproperty 'Description','OTHER PRODUCTION REPORTING ENTITY: A production reporting entity not explicitly defined in the PPDM production model.','USER','dbo','TABLE','PDEN_OTHER';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY AS POOL: This table facilitates the representation of a pool as a production reporting entity. It allows a different set of identifiers and relationships for production reporting purposes. For instance, an organization may have an internal identifier for a pool that is different than what is used by the organization or its business partners for reporting purposes.','USER','dbo','TABLE','PDEN_POOL';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY AS WELL STRING: This table facilitates the representation of a well string as a production reporting entity. It allows a different set of identifiers and relationships for production reporting purposes. For instance, an organization mayhave an internal identifier for a well string that is different than what is used by the organization or its business partners for reporting purposes.','USER','dbo','TABLE','PDEN_PROD_STRING';
go

execute sp_addextendedproperty 'Description','PRODUCTION STRING TO PDEN CROSS REFERENCE: Tracks the contribution of production from a production string to a number of PDENs. In some jurisdictions production from a number of strings are aggregated for the purposes of reporting and regulation. Sometimes this is done differently depending on the product. For instance, in Texas oil is reported on a lease basis while gas is reported on a string basis.','USER','dbo','TABLE','PDEN_PROD_STRING_XREF';
go

execute sp_addextendedproperty 'Description','PDEN PRODUCTION STRING PDEN CONTRIBUTION ALLOWABLE: Monthly (or daily) allowable values for production contributed to a specific pden. There can be many allowables in effect for production contributed to a pden.','USER','dbo','TABLE','PDEN_PR_STR_ALLOWABLE';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY AS WELL STRING FORMATION: This table facilitates the representation of a well string formation as a production reporting entity. It allows a different set of identifiers and relationships for production reporting purposes. For instance,an organization may have an internal identifier for a well string formation that is different than what is used by the organization or its business partners for reporting purposes.','USER','dbo','TABLE','PDEN_PR_STR_FORM';
go

execute sp_addextendedproperty 'Description','PDEN RESERVE ENTITY SUBTYPE: a valid subtype of production entity that is a reserve entity. This entity will allow you to track actual volumes for groups of wells.','USER','dbo','TABLE','PDEN_RESENT';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY SUBTYPE RESERVE ENTITY CLASS: This production subtype is added to allow forecast volumes to be reported for a reserve entity class. Only forecast volumes should be stored with this sub type. Actual volumes should be stored as PDEN RESENT.','USER','dbo','TABLE','PDEN_RESENT_CLASS';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY STATUS HISTORY: Contains an historical account of the operating status of the production reporting entity.','USER','dbo','TABLE','PDEN_STATUS_HIST';
go

execute sp_addextendedproperty 'Description','PDEN VOLUME ANALYSIS: Contains the parameters used for volumetric calculations .','USER','dbo','TABLE','PDEN_VOLUME_ANALYSIS';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY VOLUME DISPOSITION: A reported movement of fluid between two production entities. A relationship is established between two production entities for the duration of the transaction (movement). This relationship may be different than the usual reported or operational relationships established by the two production entities.','USER','dbo','TABLE','PDEN_VOL_DISPOSITION';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY UNIT REGIME: This table keeps track of which unit regime should be used for each production entity through the life cycle of that entity.','USER','dbo','TABLE','PDEN_VOL_REGIME';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY VOLUME REPORT SUMMARY: A summary of reported volumes for common types of fluids over a specified time period.','USER','dbo','TABLE','PDEN_VOL_SUMMARY';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY VOLUME REPORT SUMMARY - OTHER FLUIDS: Summary of reported volumes for fluids not included as categories in the volume summary report.','USER','dbo','TABLE','PDEN_VOL_SUMM_OTHER';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY AS WELL: This table facilitates the representation of a well as a production reporting entity. In the What is a Well specification, this table corresponds to WELL REPORTING STREAM. It allows a different set of identifiers and relationships for production reporting purposes. For instance, an organization may have an internal identifier for a well that is different than what is used by the organization or its business partners for reporting purposes. In some cases, it may be necessary to associate more than one well component with a well reporting stream. PDEN_WELL_REPORTING_STREAM should be used to group the components that are in the well reporting stream.','USER','dbo','TABLE','PDEN_WELL';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY WELL REPORTING STREAM: In the What is a Well specification, this table should be used to group the well components that are in a Well Reporting Stream. The Well Reporting Stream itself corresponds to PDEN_WELL.','USER','dbo','TABLE','PDEN_WELL_REPORT_STREAM';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY CROSS REFERENCE: Another means of linking production entities to support relationships not explicitly defined in the PPDM production model.','USER','dbo','TABLE','PDEN_XREF';
go

execute sp_addextendedproperty 'Description','POOL: Represents a reservoir or a group of small tracts of land brought together for the granting of a well permit under applicable spacing rules. In Canada pool almost exclusively refers to a reservoir and these codes are usually unique within a province/field. Inthe United States, these codes are unique either to the state, or to the state/field or the district. Pool definitions may be administrative (usually assigned to a production string) or geologic (usually assigned to a production string formation).','USER','dbo','TABLE','POOL';
go

execute sp_addextendedproperty 'Description','POOL ALIAS: alternate name by which the pool is known','USER','dbo','TABLE','POOL_ALIAS';
go

execute sp_addextendedproperty 'Description','POOL IN AREA: identifies any areas that the pool covers, either in total or in part. May be geopolitical, regulatory, formal, informal etc. Prior to PPDM 3.7, called POOL_IN_COUNTY.','USER','dbo','TABLE','POOL_AREA';
go

execute sp_addextendedproperty 'Description','POOL COMPONENT: This table is used to capture the relationships between pools and busines objects, such as wells, equipment, documents, seismic sets and contracts.','USER','dbo','TABLE','POOL_COMPONENT';
go

execute sp_addextendedproperty 'Description','POOL INSTRUMENT: This table identifies instruments that are created to legally define a pool. Usually pool instruments are created by a regulatory body.','USER','dbo','TABLE','POOL_INSTRUMENT';
go

execute sp_addextendedproperty 'Description','POOL VERSION: Alternate version of POOL information. The Preferred version is stored in POOL.','USER','dbo','TABLE','POOL_VERSION';
go

execute sp_addextendedproperty 'Description','POOL VERSION AREA: identifies any areas that the pool covers, either in total or in part within a version. May be geopolitical, regulatory, formal, informal etc.','USER','dbo','TABLE','POOL_VERSION_AREA';
go

execute sp_addextendedproperty 'Description','PPDM AUDIT HISTORY: Use this table to keep track of a complete audit history for information in the database. As your business rules require, you may use this to track only specific columns in the database, or all columns in a database. You can use PPDM GROUP to group and capture records that are subject to audit, if you wish.','USER','dbo','TABLE','PPDM_AUDIT_HISTORY';
go

execute sp_addextendedproperty 'Description','PPDM AUDIT HISTORY REMARK: Use this table to capture remarks about the auditing, quality control and processing of information in the database.','USER','dbo','TABLE','PPDM_AUDIT_HISTORY_REM';
go

execute sp_addextendedproperty 'Description','PPDM CHECK CONSTRAINT VALUE: this table lists the values that a column may have when the value is limited by check constraint. Check constraints are used for IND values, which may be Y or N. They are also used to support super - sub type implementations and are important in the maintenance of the integrity of these structures.','USER','dbo','TABLE','PPDM_CHECK_CONS_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE CODE VERSION: The code version table contains lookup codes and descriptions as supplied by various sources. This table pair is designed to store reference values as singles, pairs or triplets etc, depending on the structure of the underlying reference table.','USER','dbo','TABLE','PPDM_CODE_VERSION';
go

execute sp_addextendedproperty 'Description','REFERENCE CODE VERSION COLUMN: Use this table to store the values of the columns in reference sets where the PK is more than one component.','USER','dbo','TABLE','PPDM_CODE_VERSION_COLUMN';
go

execute sp_addextendedproperty 'Description','REFERENCE CODE VERSION: The code version table contains lookup codes and descriptions as supplied by various sources. Each code in a data field describes the meaning of a number, letter, abbreviation or mnemonic. In the case where use rules for each version are very simple, you may use the FK to owner, procedure, application etc to indicate where each version should be used. If use rules are more complicated, you will need to use PPDM VERSION GROUP to capture this information.','USER','dbo','TABLE','PPDM_CODE_VERSION_USE';
go

execute sp_addextendedproperty 'Description','REFERENCE CODE VERSION CROSS REFERENCE: Use this table to keep track of relationships between reference values, such as equivalences, replacements, granularity relationships (is a kind of) and so on.','USER','dbo','TABLE','PPDM_CODE_VERSION_XREF';
go

execute sp_addextendedproperty 'Description','PPDM COLUMN INFORMATION: Contains meta data regarding the columns in the PPDM schema. This is information designed to assist in the units of measure module.','USER','dbo','TABLE','PPDM_COLUMN';
go

execute sp_addextendedproperty 'Description','PPDM COLUMN ALIAS or SYNONYM: Alternate identifiers for a column, such as names shown in reports or other displays. Can be in alternate languages if desired.','USER','dbo','TABLE','PPDM_COLUMN_ALIAS';
go

execute sp_addextendedproperty 'Description','PPDM CONSTRAINT: This table lists the primary, foreign, unique and not null constraints that are applied against a table.','USER','dbo','TABLE','PPDM_CONSTRAINT';
go

execute sp_addextendedproperty 'Description','PPDM CONSTRAINT COLUMN: This table lists the columns that are included in the constraint, the sequence of columns in a constraint and identifies referenced columns for foreign keys.','USER','dbo','TABLE','PPDM_CONS_COLUMN';
go

execute sp_addextendedproperty 'Description','PPDM_DATA_STORE: Allow for the grouping of tables which may use different units of measure as defaults.','USER','dbo','TABLE','PPDM_DATA_STORE';
go

execute sp_addextendedproperty 'Description','PPDM COLUMN DOMAIN: Meta data dealing with commonalities of columns (their makeup). These common qualities are known as the domain spanning the columns.','USER','dbo','TABLE','PPDM_DOMAIN';
go

execute sp_addextendedproperty 'Description','PPDM EXCEPTION: Table to hold any exceptions or constraint violations. These typically occur when referential integrity constraints have been disabled (to allow for a mass load or update) and then re-enabled. Any rows which violate a const raint will be recordedin the exception table. This allows the ability to correct the row (or delete it). Thus the constraints can be re-enabled.','USER','dbo','TABLE','PPDM_EXCEPTION';
go

execute sp_addextendedproperty 'Description','PPDM GROUP: This table can be used to track associations between columns in the data model and logical groupings. For example, you could use this table to group all columns that fall in a particular domains, such as depth, or all the columns used by a software application, or the columns used in a particular report.','USER','dbo','TABLE','PPDM_GROUP';
go

execute sp_addextendedproperty 'Description','PPDM GROUP OBJECT: this table allows you to group system or business objects together into logical sets. You can use this table to link tables, columns, procedure, business rules and more into sets. Sets may be used for reporting, metrics calculation, data quality checking and more.','USER','dbo','TABLE','PPDM_GROUP_OBJECT';
go

execute sp_addextendedproperty 'Description','PPDM GROUP OWNER: This table can be used to track who the owners of a group are. A group may be owned by one or more applications. Each group may also be owned by one or more business assiciates, each with a different role. For example, you may track the owner of the business value of data, the technical application of data, the data management of the data, the data loading etc.','USER','dbo','TABLE','PPDM_GROUP_OWNER';
go

execute sp_addextendedproperty 'Description','PPDM GROUP REMARK: Use this table to record narrative remarks about any PPDM Group. This table is intended to help document and describe groups fully.','USER','dbo','TABLE','PPDM_GROUP_REMARK';
go

execute sp_addextendedproperty 'Description','PPDM GROUP CROSS REFERENCE: This table can be used to keep track of relationships between groups, such as hierarchical relationships, component (part of) relationships, replacements or deprecations etc. Use the XREF TYPE column to describe why the relationshipwas created.','USER','dbo','TABLE','PPDM_GROUP_XREF';
go

execute sp_addextendedproperty 'Description','PPDM INDEX: This table lists the indexes provided through the DDL. Members may populate this table to show the indexes that are applied for a specific implementation. The PPDM Association provides a basic set of starter indexes for model delivery.','USER','dbo','TABLE','PPDM_INDEX';
go

execute sp_addextendedproperty 'Description','PPDM INDEX COLUMN: This table lists the columns included in each index, together with the column sequence.','USER','dbo','TABLE','PPDM_INDEX_COLUMN';
go

execute sp_addextendedproperty 'Description','PPDM SYSTEM DETAILED MAPPING: Use this table to track mappings between systems. You can map between tables, columns or schema in any combination you require.','USER','dbo','TABLE','PPDM_MAP_DETAIL';
go

execute sp_addextendedproperty 'Description','PPDM MAP LOAD: Use this table to capture the different components that are used in a model to model transformation/loading.','USER','dbo','TABLE','PPDM_MAP_LOAD';
go

execute sp_addextendedproperty 'Description','PPDM MAP LOAD ERROR: Use this table to keep track of the errors that are encountered during a load, and the resolution for each error.','USER','dbo','TABLE','PPDM_MAP_LOAD_ERROR';
go

execute sp_addextendedproperty 'Description','PPDM SYSTEM DETAILED MAPPING RULE: Use this table to track rules that govern the mapping or migration between systems. For example, if the value of a column should be derived from a colum value or a column name, or based on a calculation from an existing column,you may store the rule here. If a value in a mapped column is also validated against a reference table, you may store the name of the reference table and validated column here.','USER','dbo','TABLE','PPDM_MAP_RULE';
go

execute sp_addextendedproperty 'Description','MEASUREMENT SYSTEM DEFINITION: Name and description for valid systems for units of measure. For example, the International System of Units (SI). In the sample data, where a unit of measure is part of SI, SI will be used - otherwise, the precedence will be: SI, Imperial, US Customary, MKS, CGS, Historical.','USER','dbo','TABLE','PPDM_MEASUREMENT_SYSTEM';
go

execute sp_addextendedproperty 'Description','PPDM METRIC: Use this table to define the kinds of metrics being managed. Metrics may relate to software, data in a database, XML schema, projects etc. Metrics are typically used to measure performance or progress.','USER','dbo','TABLE','PPDM_METRIC';
go

execute sp_addextendedproperty 'Description','PPDM METRIC COMPONENT: Use this table to track the business objects relevant to a metric, such as a list of the wells included in a metric, or the tables and columns that are being measured.','USER','dbo','TABLE','PPDM_METRIC_COMPONENT';
go

execute sp_addextendedproperty 'Description','PPDM METRIC VALUE: Use this table to capture the values of the metrics that are being monitored. You may capture the number of objects loaded or quality controlled, the number of software licenses in use, the quantity of objects in a repository etc.','USER','dbo','TABLE','PPDM_METRIC_VALUE';
go

execute sp_addextendedproperty 'Description','PPDM OBJECT STATUS: This table allows you to track the status of various data base objects, such as tables, columns, constraints, indexes, procedures etc as they change over time. Your implementation may choose to track a complete history or a partial history of this information. Status information can be useful when diagnosing database, system, application or data problems.','USER','dbo','TABLE','PPDM_OBJECT_STATUS';
go

execute sp_addextendedproperty 'Description','PPDM PROCEDURE: Use this table to track procedures used for a system or table. May be a stored procedure, called procedure, function etc.','USER','dbo','TABLE','PPDM_PROCEDURE';
go

execute sp_addextendedproperty 'Description','PPDM PROPERTY COLUMN CONTROL: Each row in this table describes how a column in the used table should be managed. Each property set may require the use of one or more columns in the used table. Numeric values should specify which columns to use and the data type, length and precision for each value. Also specify the preferred units of measure. If the used column should be validated against a reference table, the name of the reference table is specificed. Note that references cannot be validated through referential integrity, so great care must betaken to ensure that corrupt data does not enter the table.','USER','dbo','TABLE','PPDM_PROPERTY_COLUMN';
go

execute sp_addextendedproperty 'Description','TABLE PROPERTY CONTROL: This table can be used to help you control how a vertical table is implemented, by determining preferred units of measure, data entry types and other specifications for each kind of property that is defined in a vertical table. We recommend that each row capture property set definitions for only one table. Each used table will probably require more than one property set.','USER','dbo','TABLE','PPDM_PROPERTY_SET';
go

execute sp_addextendedproperty 'Description','PPDM QUALITY CONTROL: Use this table to track the processes and statuses associated with reviewing and validating information that is contained in other tables. Caution must be employed when implementing this table, as a row in this table may not necessarily relate to a value that is currently in the database. During an update process, the value in a column may be changed to reflect what is known about the object being investigated. Use the CURRENT VALUE % columns to keep track of what the value is for the purposes of this record.','USER','dbo','TABLE','PPDM_QUALITY_CONTROL';
go

execute sp_addextendedproperty 'Description','PPDM QUANTITY: Describes the type of value that is being measured, The Conventions of ASTM/IEEE SI-10 are used wherever possible. For example, length, luminance, mass density, power.','USER','dbo','TABLE','PPDM_QUANTITY';
go

execute sp_addextendedproperty 'Description','PPDM QUANTITY ALIAS: Alternate names or identifiers for a PPDM quantity. For example, distance is an alias for length. .','USER','dbo','TABLE','PPDM_QUANTITY_ALIAS';
go

execute sp_addextendedproperty 'Description','PPDM RULE: A meta table that captures rules governing the use of a system. Rules may be policies, practices, procedures or business rules. Rules may be enforced through data base rules, application logic, user interfaces or best business practices. They may also define the way tasks are to be performed. When setting up a new company or an organization, the PPP can help you determine what your corporate objectives should be (polices), what your organizational structure needs to support (procedures) and what key roles and responsibilities are needed (practices).','USER','dbo','TABLE','PPDM_RULE';
go

execute sp_addextendedproperty 'Description','PPDM RULE ALIASES: This table may be used to store aliases, such as alternate names, codes or identifiers for a business object. All versions of an objects identification should be stored here, including the preferred version.','USER','dbo','TABLE','PPDM_RULE_ALIAS';
go

execute sp_addextendedproperty 'Description','PPDM RULE COMPONENT: Use this table to connect PPDM rules to the business objects they influence or are influenced by. Can be used to describe the geographic area in which a rule applies, or the specific wells that a rule controls etc.','USER','dbo','TABLE','PPDM_RULE_COMPONENT';
go

execute sp_addextendedproperty 'Description','PPDM RULE DETAIL: Use this table to describe details about a rule. These rules may be textual or they may be captured as numbers, such as an allowable range of numbers that a column may contain. Some business rules may require complexity in these tables to fully describe.','USER','dbo','TABLE','PPDM_RULE_DETAIL';
go

execute sp_addextendedproperty 'Description','PPDM RULE ENFORCEMENT: Use this table to define how the rule is enforced. Rules may be enforced by the data base DDL, Software application logic, manual procedure etc.','USER','dbo','TABLE','PPDM_RULE_ENFORCEMENT';
go

execute sp_addextendedproperty 'Description','PPDM_RULE_REMARK: Use this table to record narrative remarks about any PPDM Rule. This table is intended to help document and describe rules. Remarks can be grouped together using a sequence number and the rule ID to describe more complex rules.','USER','dbo','TABLE','PPDM_RULE_REMARK';
go

execute sp_addextendedproperty 'Description','PPDM RULE CROSS REFERENCE: Use this table to keep track of cases where the use of a rule may depend on the outcome of another rule implementation. For example, the second rule is only enforced if conditions for the first have passed successfully (or failed). Other uses of this table include replacement of rules, refining rules etc.','USER','dbo','TABLE','PPDM_RULE_XREF';
go

execute sp_addextendedproperty 'Description','PPDM SCHEMA ENTITY: Use this table to describe an XML schema or flat file system. You can define individual elements, attributes or groups of objects. Associate objects with each other using PPDM SCHEMA GROUP.','USER','dbo','TABLE','PPDM_SCHEMA_ENTITY';
go

execute sp_addextendedproperty 'Description','SYSTEM ENTITY ALIAS: Alternate names, codes and identifiers that are used to reference components in a system such as a database or an XML schema.','USER','dbo','TABLE','PPDM_SCHEMA_ENTITY_ALIAS';
go

execute sp_addextendedproperty 'Description','PPDM SCHEMA GROUP: use this table to group entities in a schema or file into logical units, such as the relationship between an element and its attributes, parent child relationships, siblings, sequencing elements.','USER','dbo','TABLE','PPDM_SCHEMA_GROUP';
go

execute sp_addextendedproperty 'Description','PPDM SOFTWARE APPLICATION: the name of a software application, such as Microsoft Word.','USER','dbo','TABLE','PPDM_SW_APPLICATION';
go

execute sp_addextendedproperty 'Description','PPDM SOFTWARE APPLICATION ALIAS: All possible names, codes and other identifiers can be stored here.','USER','dbo','TABLE','PPDM_SW_APPLIC_ALIAS';
go

execute sp_addextendedproperty 'Description','PPDM SOFTWARE APPLICATION COMPONENT: use this table to keep track of what data a software application has access to.  This can be defined to the level of rows of data if desired, but more commonly, one would use the references to PPDM SYSTEM,  PPDM TABLE, and perhaps PPDM COLUMN to identify what parts of a data store are used by each application.','USER','dbo','TABLE','PPDM_SW_APPLIC_COMP';
go

execute sp_addextendedproperty 'Description','SOFTWARE APPLICATION BUSINESS ASSOCIATE: Use this table to keep track of the business associates (company and people) who have a role in the acquisition, purchase, deployment, use and support for a software application.','USER','dbo','TABLE','PPDM_SW_APP_BA';
go

execute sp_addextendedproperty 'Description','SOFTWARE APPLICATION FUNCTIONS: use this table to track the functions or roles played by a software application. This allows you to group software for management of licenses, data transfers, business functions etc.','USER','dbo','TABLE','PPDM_SW_APP_FUNCTION';
go

execute sp_addextendedproperty 'Description','REFERENCE SOFTWARE APPLICATION CROSS REFERENCE: Use this table to cross reference applications to each other. This is useful to keep track of software products that replace others, or products that provide a data input to another application, or accept an input from another. You can also use it to indicate dependencies in workflows (which application is used before, after or in conjunction with another).','USER','dbo','TABLE','PPDM_SW_APP_XREF';
go

execute sp_addextendedproperty 'Description','PPDM SYSTEM: This table defines a data storage system definition, such as a database or XML schema. For example, an implementation of PPDM 3.7 would be a system and an implementation of PPDM 3.8 would be a different system. Use the meta model tables to define thetables, columns and constraints in each system, and to provide business rules and mapping for sharing data among different systems.','USER','dbo','TABLE','PPDM_SYSTEM';
go

execute sp_addextendedproperty 'Description','SYSTEM ALIAS: Alternate names, codes and identifiers that are used to reference a system such as a database or an XML schema.','USER','dbo','TABLE','PPDM_SYSTEM_ALIAS';
go

execute sp_addextendedproperty 'Description','PPDM SYSTEM APPLICATION: this table contains a list of all the software applications that use a particular data store system.','USER','dbo','TABLE','PPDM_SYSTEM_APPLICATION';
go

execute sp_addextendedproperty 'Description','SYSTEM MAP: Use this table to track high level information about mappings between systems. This table tracks general information about a mapping project.','USER','dbo','TABLE','PPDM_SYSTEM_MAP';
go

execute sp_addextendedproperty 'Description','PPDM TABLE: A meta data table which contains information regarding the tables contained within the PPDM schema.','USER','dbo','TABLE','PPDM_TABLE';
go

execute sp_addextendedproperty 'Description','PPDM TABLE ALIAS or SYNONYM: Alternate identifiers for a table, usually referred to a synonyms. The PPDM association provides a standard set of synonyms for PPDM tables with model DDL.','USER','dbo','TABLE','PPDM_TABLE_ALIAS';
go

execute sp_addextendedproperty 'Description','PPDM TABLE AUDIT HISTORY: Use this table to track data that has been deleted from the database. Where specific columns are deleleted or modified, you can use PPDM AUDIT HISTORY. You can use the DELETE RECORD to store the original contents of the row, preferably in XML format, in the event you want to restore the data later.','USER','dbo','TABLE','PPDM_TABLE_HISTORY';
go

execute sp_addextendedproperty 'Description','PPDM UNIT CONVERSION: This table stores numeric data used to convert between different units of measure, following the formula: TO_UOM=(PRE_OFFSET+FROM_UOM)*(FACTOR_NUMERATOR/FACTOR_DENOMINATOR)+POST_OFFSET. Note that the table supports only conversions that are linear.','USER','dbo','TABLE','PPDM_UNIT_CONVERSION';
go

execute sp_addextendedproperty 'Description','PPDM UNIT OF MEASURE: Table containing all valid units of measure and describes what system they belong to as well as the quantity associated with the unit. A quantity is a "type" of unit, for example, length, pressure, and temperature are all valid quantities.','USER','dbo','TABLE','PPDM_UNIT_OF_MEASURE';
go

execute sp_addextendedproperty 'Description','PPDM UNITS OF MEASURE ALIAS: Table of aliases, or other common names for Units of measure.','USER','dbo','TABLE','PPDM_UOM_ALIAS';
go

execute sp_addextendedproperty 'Description','PPDM VOL MEAS CONV: This table tracks The specific conversion factors from one unit of measure to another, consistant with the standard pressure and temperature associated with the unit of measure.','USER','dbo','TABLE','PPDM_VOL_MEAS_CONV';
go

execute sp_addextendedproperty 'Description','PPDM VOL MEAS REGIME: This table tracks the volume regimes set up to handle sets of conversion factors. Separate volume regimes are required when the standard pressure and temperature used to measure oil and gas volumes are different .','USER','dbo','TABLE','PPDM_VOL_MEAS_REGIME';
go

execute sp_addextendedproperty 'Description','PPDM VOL MEAS USE: This table tracks the political and geographic extents applicable to a specific volume regime.','USER','dbo','TABLE','PPDM_VOL_MEAS_USE';
go

execute sp_addextendedproperty 'Description','PRODUCTION LEASE OR UNIT: Represents an alias used for production reporting purposes of a mineral lease or unitization agreement (unit). Note that lease and unit are often treated synonymously for production reporting purposes and it is sometimes not possible to distinguish between them on the basis of production reports. For this reason they are treated as a single entity. A lease is the right obtained for the purpose of exploration and development of hydrocarbons. Such leases typically describe the right to produce by surface boundaries or subsurface intervals or boundaries. A unit is a combination of leases, usually contiguous, involving potential or producing mineral properties, for the purpose of efficient or economic operation.','USER','dbo','TABLE','PROD_LEASE_UNIT';
go

execute sp_addextendedproperty 'Description','PROD LEASE UNIT ALIAS: alternate name by which the lease unit is known.','USER','dbo','TABLE','PROD_LEASE_UNIT_ALIAS';
go

execute sp_addextendedproperty 'Description','PRODUCTION LEASE UNIT IN COUNTY: Identifies the counties that a production lease or unit covers in full or in part.','USER','dbo','TABLE','PROD_LEASE_UNIT_AREA';
go

execute sp_addextendedproperty 'Description','PROD LEASE UNIT VERSION: Alternate informatin about the lease unit from different sources. The preferred version is stored in PROD LEASE UNIT.','USER','dbo','TABLE','PROD_LEASE_UNIT_VERSION';
go

execute sp_addextendedproperty 'Description','PRODUCTION LEASE UNIT VERSION AREA: Alternate information about the lease unit area from different sources.','USER','dbo','TABLE','PROD_LEASE_UNIT_VER_AREA';
go

execute sp_addextendedproperty 'Description','PRODUCTION STRING: A string of production tubing providing a conduit from the surface to zero or more well completions. A production string allows the fluid exchange between the well completion and the wellhead. The physical configuration of a production stringcan vary over time, and the individual well completions associated with a production string can also change as a result of cementing or reworks. A well may have more than one production string.','USER','dbo','TABLE','PROD_STRING';
go

execute sp_addextendedproperty 'Description','PRODUCTION STRING ALIAS: The Alias table contains names and identifiers that a production string may otherwise be known as. This would include previous or alternate identifiers assigned by a regulatory agency and the reason for the alias. May also include aliases used by software applications or other parties.','USER','dbo','TABLE','PROD_STRING_ALIAS';
go

execute sp_addextendedproperty 'Description','PRODUCTION STRING COMPONENT: This table is used to capture the relationships between production strings and busines objects, such as wells, equipment, documents, seismic sets and contracts.','USER','dbo','TABLE','PROD_STRING_COMPONENT';
go

execute sp_addextendedproperty 'Description','PRODUCTION STRING FORMATION: Represents a specific layer of reservoir rock through which fluids flow from a reservoir into a string of production tubing. This table can be used to prorate production from a production string back to individual formations.','USER','dbo','TABLE','PROD_STRING_FORMATION';
go

execute sp_addextendedproperty 'Description','PRODUCTION STRING FORMATION ALIAS: The Alias table contains names, codes and identifiers that a production string formation may otherwise be known as. This would include previous or alternate identifiers assigned by a regulatory agency and the reason for the alias. May also include software or partner aliases etc.','USER','dbo','TABLE','PROD_STRING_FORM_ALIAS';
go

execute sp_addextendedproperty 'Description','PRODUCTION STRING STATUS: Contains an historical account of the operating status of the production string.','USER','dbo','TABLE','PROD_STR_STAT_HIST';
go

execute sp_addextendedproperty 'Description','PROJECT: a project is an organised work effort directed towards accomplishing a recognised set of objectives or goals. In the PPDM context, a project may be described in terms of its duration, area and location, funding, participants or the PPDM elements that wereincluded in or required for the work.','USER','dbo','TABLE','PROJECT';
go

execute sp_addextendedproperty 'Description','PROJECT ALIAS: Alternate names and codes that this project is known by.','USER','dbo','TABLE','PROJECT_ALIAS';
go

execute sp_addextendedproperty 'Description','PROJECT BUSINESS ASSOCIATE: this table allows a project to be associated with all Business Associates who have a role in the project. These BAs may be participants, partners, regulatory, service providers etc.','USER','dbo','TABLE','PROJECT_BA';
go

execute sp_addextendedproperty 'Description','PROJECT BUSINESS ASSOCIATE ROLE: this table can be used to list multiple roles that are held or carried out by a Business Associate through the duration of a project. Roles may include interpreter, project manager, technical support etc.','USER','dbo','TABLE','PROJECT_BA_ROLE';
go

execute sp_addextendedproperty 'Description','PROEJCT COMPONENT: Lists the business objects that are associated with a project, such as wells, seismic, land, cost centers etc. Flags indicate whether each business object was used as input to the project (as in well logs) or created as output from the project (as in interepreted values)','USER','dbo','TABLE','PROJECT_COMPONENT';
go

execute sp_addextendedproperty 'Description','PROJECT CONDITION: This table lists conditions that must be met for the project to proceed. May be completion of another project, or an external condition, such as the operational state of a facility.','USER','dbo','TABLE','PROJECT_CONDITION';
go

execute sp_addextendedproperty 'Description','PROJECT EQUIPMENT: Allows a project to be associated with equipment that is either a real specific piece of equipment (EQUIPMENT) or a kind of equipment (CAT EQUIPMENT). Description of the role(s) played by the equipment is defined in PROJECT EQUIP ROLE.','USER','dbo','TABLE','PROJECT_EQUIPMENT';
go

execute sp_addextendedproperty 'Description','PROJECT EQUIPMENT ROLE: this table can be used to list multiple roles that are held or carried out by a equipment that is either a real specific piece of equipment (EQUIPMENT) or a kind of equipment (CAT EQUIPMENT) through the duration of a project. Rolesmay include safety equipment, computer equipment, production equipment etc.','USER','dbo','TABLE','PROJECT_EQUIP_ROLE';
go

execute sp_addextendedproperty 'Description','PROJECT PLAN: a project is an organised work effort directed towards accomplishing a recognised set of objectives or goals. Use this table to define a generalized or approved set of steps to be undertaken in a certain set of circumstances. These tables representthe approved procedures for your organization.','USER','dbo','TABLE','PROJECT_PLAN';
go

execute sp_addextendedproperty 'Description','PROJECT ALLOWED STEP: this table describes the steps that are authorized or expected to be completed under a project plan. In certain cases, this table may be used to ensure that standard processes are used for specific business operations.','USER','dbo','TABLE','PROJECT_PLAN_STEP';
go

execute sp_addextendedproperty 'Description','PROJECT ALLOWED STEP CROSS REFERENCE: this table may be used to order steps in a project plan. This ordering may or may not be linear. For example, after the successful completion of step 2, three new steps may be initiated concurrently.','USER','dbo','TABLE','PROJECT_PLAN_STEP_XREF';
go

execute sp_addextendedproperty 'Description','PROJECT STATUS: This table allows you to keep track of the status of a project as it changes over time, from the perspective of various roles. STATUS TYPE defines the type of perspective you are looking at (operational, financial, legal) and STATUS defines the status from that perspective (PENDING, ACTIVE, COMPLETE, NOT APPROVED).','USER','dbo','TABLE','PROJECT_STATUS';
go

execute sp_addextendedproperty 'Description','PROJECT STEP: Lists the actual steps that are completed over the course of a project, together with details about when that step was due, when started and some basic project management information. Information about the business associates who completed the step is found in the subordinate table PROJECT STEP BA.','USER','dbo','TABLE','PROJECT_STEP';
go

execute sp_addextendedproperty 'Description','PROJECT STEP BUSINESS ASSOCIATE: Lists the business associates who were involved in completing this step of a project. More than one person and multiple roles may be listed here.','USER','dbo','TABLE','PROJECT_STEP_BA';
go

execute sp_addextendedproperty 'Description','PROJECT STEP EQUIPMENT: Tracks the equipment used in a specific project step in a specific role. For example, can track the vehicle used for a crew change or the instrument used in a specific analysis.','USER','dbo','TABLE','PROJECT_STEP_EQUIP';
go

execute sp_addextendedproperty 'Description','PROJECT STEP TIME: this table tracks time spent on a project step, often used for metrics.','USER','dbo','TABLE','PROJECT_STEP_TIME';
go

execute sp_addextendedproperty 'Description','PROJECT STEP CROSS REFERENCE: this table is used to track relationships between steps, such as precursors, followers, optional paths etc. Used to create a more robust set of data than the simple rule in PROJECT STEP.','USER','dbo','TABLE','PROJECT_STEP_XREF';
go

execute sp_addextendedproperty 'Description','PROJECT CONDITION: This table lists conditions that must be met for the project to proceed. May be completion of another step in the project, or an external condition, such as the operational state of a facility.','USER','dbo','TABLE','PROJ_STEP_CONDITION';
go

execute sp_addextendedproperty 'Description','PRODUCTION LEASE UNIT STRING HISTORY: Identifies historical relationships (assignments) of production strings to a lease or unit.','USER','dbo','TABLE','PR_LSE_UNIT_STR_HIST';
go

execute sp_addextendedproperty 'Description','PRODUCTION STRING FORMATION COMPLETION: Tracks the complex relationships between production strings and well completion activities. A production string formation may have one or more completions associated with it (such as in the case of com-mingled production from multiple sands). The relationship is versionable over time, as some completions may be activated and deactivated mechanically or by changing conditions in the formation that is producing.','USER','dbo','TABLE','PR_STR_FORM_COMPLETION';
go

execute sp_addextendedproperty 'Description','PRODUCTION STRING FORMATION STATUS HISTORY: Shows the history of how a part icular formation (layer) was configured to contribute production to a production string.','USER','dbo','TABLE','PR_STR_FORM_STAT_HIST';
go

execute sp_addextendedproperty 'Description','RATE AREA: this table tracks the relationships between rate schedules and the areas in which they are in effect.','USER','dbo','TABLE','RATE_AREA';
go

execute sp_addextendedproperty 'Description','RATE and FEE SCHEDULE: A schedule for payments as set up by a business associate such as a service provider, jurisdiction or regulatory agency. Fee schedules are often used to administer the rates for rentals or for services provided. .','USER','dbo','TABLE','RATE_SCHEDULE';
go

execute sp_addextendedproperty 'Description','RATE SCHEDULE COMPONENT: This table is used to capture the relationships between rate schedules and busines objects, such as wells, equipment, documents, seismic sets and contracts.','USER','dbo','TABLE','RATE_SCHEDULE_COMPONENT';
go

execute sp_addextendedproperty 'Description','RATE SCHEDULE CROSS REFERENCE: use this table to associate schedules with each other.','USER','dbo','TABLE','RATE_SCHEDULE_XREF';
go

execute sp_addextendedproperty 'Description','RATE and FEE SCHEDULE DETAIL: this table is used to capture detailed costs associated with the fee schedule.','USER','dbo','TABLE','RATE_SCHED_DETAIL';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ACCESS CONDITION: All possible names, codes and other identifiers for an object should be stored in the alias table. Most subject areas in PPDM have an alias table. The alias table is owned by the parent of each subject, or by each reference table. ALIAS tables cannot be populated until the parent table has been populated (and committed).','USER','dbo','TABLE','RA_ACCESS_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ACCOUNTING PROCEDURE TYPE: All possible names, codes and other identifiers for the type of accounting procedure','USER','dbo','TABLE','RA_ACCOUNT_PROC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ACTIVITY SET TYPE: All possible names, codes and other identifiers for the type of activity set, such as standard, corporate, regulatory etc.','USER','dbo','TABLE','RA_ACTIVITY_SET_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ACTIVITY TYPE: All possible names, codes and other identifiers for the type of activity that caused the movement of fluids to occur such as production, injection, flaring, sales, etc.','USER','dbo','TABLE','RA_ACTIVITY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ADDITIVE METHOD: All possible names, codes and other identifiers for the method used for adding the additive to the well bore.','USER','dbo','TABLE','RA_ADDITIVE_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL TREATMENT ADDITIVE TYPE: All possible names, codes and other identifiers for the type of additive used in the treatment fluid during the acidizing job. For example, acid, detergent, ChemGel etc.','USER','dbo','TABLE','RA_ADDITIVE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ADDRESS TYPE: All possible names, codes and other identifiers for the type of business associate address. For example shipping, billing, sales...','USER','dbo','TABLE','RA_ADDRESS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS AIR CRAFT TYPE: All possible names, codes and other identifiers for the type of aircraft described. Examples may be general (jet, two engine, helicopter) or very specific, such as the list found here http://www.airlinecodes.co.uk/acrtypes.htm.','USER','dbo','TABLE','RA_AIRCRAFT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS AIR GAS COD: All possible names, codes and other identifiers for the type of fluid supplied by the drilling compressor. For example, the fluid can be Air or Gas.','USER','dbo','TABLE','RA_AIR_GAS_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS REASON TYPE: All possible names, codes and other identifiers for the purpose or reason for a given alias. For example a well alias may be assigned to the well because of a name change or amendment to the identifier. A business associate alias may as a result of a merger or name change.','USER','dbo','TABLE','RA_ALIAS_REASON_TYPE';
go

execute sp_addextendedproperty 'Description','ALIAS: All possible names, codes and other identifiers for an object should be stored in the alias table. Most subject areas in PPDM have an alias table. The alias table is owned by the parent of each subject, or by each reference table. ALIAS tables cannot bepopulated until the parent table has been populated (and committed).','USER','dbo','TABLE','RA_ALIAS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ALLOCATION FACTOR TYPE: All possible names, codes and other identifiers for the type of allocation factor that is used in calculations to attribute (allocate) a measured movement of fluid to a number of production entities.','USER','dbo','TABLE','RA_ALLOCATION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ALLOWABLE EXPENSE TYPE: All possible names, codes and other identifiers for the type of allowable expenses defined in a contract.','USER','dbo','TABLE','RA_ALLOWABLE_EXPENSE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS PROPERTY: All possible names, codes and other identifiers that identifies the compositional and/or physical properties being analyzed. For example, the types of properties subjected to analysis may be BTU, Gas composition, Mole percentage.','USER','dbo','TABLE','RA_ANALYSIS_PROPERTY';
go

execute sp_addextendedproperty 'Description','ALIAS: All possible names, codes and other identifiers for an object should be stored in the alias table. Most subject areas in PPDM have an alias table. The alias table is owned by the parent of each subject, or by each reference table. ALIAS tables cannot be populated until the parent table has been populated (and committed).','USER','dbo','TABLE','RA_ANL_ACCURACY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS BUSINESS ASSOCIATE ROLE TYPE: All possible names, codes and other identifiers for the type of role that a business associate plays or may play during a sample analysis. Examples include technician, scientist, reviewer, laboratory, conducted for, document preparation etc.','USER','dbo','TABLE','RA_ANL_BA_ROLE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS CALCULATION EQUIVALENCE TYPE: All possible names, codes and other identifiers for the kind of relationship or equivalence that is defined for two calculation methods. May indicate methods that provide similar results, methods tobe used in preference over another, methods that replace deprecated methods etc.','USER','dbo','TABLE','RA_ANL_CALC_EQUIV_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS LIQUID CHROMATOGRAPHY PROPERTY TYPE: All possible names, codes and other identifiers for the type of chromatography property measured.','USER','dbo','TABLE','RA_ANL_CHRO_PROPERTY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SAMPLE ANALYSIS COMPONENT TYPE: All possible names, codes and other identifiers for the type of component associated with a sample analysis.','USER','dbo','TABLE','RA_ANL_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS CONFIDENCE TYPE: All possible names, codes and other identifiers for the level of confidence or certainty for an analysis value. Various systems for measurement are defined in literature, and may be text based (CERTAIN, PROBABLE, UNCERTAIN) or number based. This value tends to be subjective, and indicates the level of trust the analyst has in the result.','USER','dbo','TABLE','RA_ANL_CONFIDENCE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DETAIL REFERENCE VALUE TYPE: All possible names, codes and other identifiers for the type of reference value is captured here.','USER','dbo','TABLE','RA_ANL_DETAIL_REF_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS DETAIL TYPE: All possible names, codes and other identifiers for the type of technical analysis result that has been captured.','USER','dbo','TABLE','RA_ANL_DETAIL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS VALUE CODE: All possible names, codes and other identifiers for the code assigned to the value by observation, in cases where numeric values are not used.','USER','dbo','TABLE','RA_ANL_ELEMENT_VALUE_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS VALUE TYPE: All possible names, codes and other identifiers for the type of values for the analysis, in cases where the value is text based.','USER','dbo','TABLE','RA_ANL_ELEMENT_VALUE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS EQUIPMENT ROLE: All possible names, codes and other identifiers for the valid roles played by equipment during an analysis. May include grinding, polishing, pyrolysis, spectroscopy etc.','USER','dbo','TABLE','RA_ANL_EQUIP_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS FORMULA TYPE: All possible names, codes and other identifiers for the type of formula that has been described. A common kind of calculation method or formula type are RATIOS.','USER','dbo','TABLE','RA_ANL_FORMULA_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS VALUE TYPE: All possible names, codes and other identifiers for the type of values for the analysis, in cases where the value is text based.','USER','dbo','TABLE','RA_ANL_GAS_CHRO_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS PROPERTY: All possible names, codes and other identifiers for the compositional and/or physical properties being analyzed. For example, the types of properties subjected to analysis may be BTU, Gas composition, Mole percentage.','USER','dbo','TABLE','RA_ANL_GAS_PROPERTY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS GAS PROPERTY VALUE CODE: All possible names, codes and other identifiers for the code assigned to the analysis property by observation, in cases where numeric values are not used.','USER','dbo','TABLE','RA_ANL_GAS_PROPERTY_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS METHOD EQUIVALENCE TYPE: All possible names, codes and other identifiers for the kind of relationships between analysis methods, indicating whether two methods are exactly the same, nearly the same, a process that supercedes (and hopefully improves) on an older process, a process that is recommended in lieu of another etc.','USER','dbo','TABLE','RA_ANL_METHOD_EQUIV_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS METHOD SET TYPE: All possible names, codes and other identifiers for the type or kind of analysis method set that has been described, such as Isotope analysis, mineral analysis, organic geochemistry, paleontological analysis, biostratigraphic analysis, total organic carbon analysis etc.','USER','dbo','TABLE','RA_ANL_METHOD_SET_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS MISSING REPRESENTATION TYPE: All possible names, codes and other identifiers for valid representations that are used by labs when a measurement is missing because it is out of range. Usually this is the result of equipment limitations.','USER','dbo','TABLE','RA_ANL_MISSING_REP';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS NULL REPRESENTATION: All possible names, codes and other identifiers for the case where a reading or measurement or calculation was not provided.','USER','dbo','TABLE','RA_ANL_NULL_REP';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS PROPERTY VALUE CODE: All possible names, codes and other identifiers for the code assigned to the analysis property by observation, in cases where numeric values are not used.','USER','dbo','TABLE','RA_ANL_OIL_PROPERTY_CODE';
go

execute sp_addextendedproperty 'Description','ALIAS: All possible names, codes and other identifiers for an object should be stored in the alias table. Most subject areas in PPDM have an alias table. The alias table is owned by the parent of each subject, or by each reference table. ALIAS tables cannot be populated until the parent table has been populated (and committed).','USER','dbo','TABLE','RA_ANL_PARAMETER_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS PROBLEM RESOLUTION METHOD: All possible names, codes and other identifiers for the method used to resolve a problem encountered during analysis. Could include re-running the samples, calibrating equipment, collecting a new sample batch, altering parameters and so on.','USER','dbo','TABLE','RA_ANL_PROBLEM_RESOLUTION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS PROBLEM RESULT: All possible names, codes and other identifiers for the type of consequence that was the outcome of the problem described. For example, results may be inaccurate, or a test may be destroyed, or results may show anomolous values.','USER','dbo','TABLE','RA_ANL_PROBLEM_RESULT';
go

execute sp_addextendedproperty 'Description','ALIAS: All possible names, codes and other identifiers for an object should be stored in the alias table. Most subject areas in PPDM have an alias table. The alias table is owned by the parent of each subject, or by each reference table. ALIAS tablescannot be populated until the parent table has been populated (and committed).','USER','dbo','TABLE','RA_ANL_PROBLEM_SEVERITY';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS PROBLEM TYPE: All possible names, codes and other identifiers for the valid problems that can and are associated with laboratory analysis. Problems may relate to equipment calibration errors, sample contamination, incorrect procedures used, technical error etc.','USER','dbo','TABLE','RA_ANL_PROBLEM_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSYS REFERENCE VALUE TYPE: In the case where a detail is referenced to some other value all possible names, codes and other identifiers forthe type of reference value is captured here. For example, the temperature of a process step may be specific to the atmospheric pressure of the container.','USER','dbo','TABLE','RA_ANL_REF_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSYS REMARK TYPE: All possible names, codes and other identifiers for the kind of remark that has been inserted about a sample analysis. Usually, this would refer to whether this was a comment about the sample, the equipment used, contamination that was found, unusual circumstances that existed etc.','USER','dbo','TABLE','RA_ANL_REMARK_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS REPEATABILITY: All possible names, codes and other identifiers for the level of repeatability for a study result. Indicates how consistently the same or a similar result will be obtained when a step is repeated. A result may be highly repeatable, but still incorrect or not trustworthy. For example a sample contaminant may affect your trust in the data, even though you get the same (incorrect) answer again and again. Equipment capabilities may also result in a highly repeatable but inaccurate result.','USER','dbo','TABLE','RA_ANL_REPEATABILITY';
go

execute sp_addextendedproperty 'Description','ANALYSIS STEP CROSS REFERENCE REASON: All possible names, codes and other identifiers for the reason why two steps are related to each other. Usually would indicate a step that follows another. Could also be used to track a new step that replaces a step that failed or did not have a satisfactory outcome.','USER','dbo','TABLE','RA_ANL_STEP_XREF';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS TOLERANCE TYPE:All possible names, codes and other identifiers for the types of tolerances for valid measurements in an analysis. Tolerances may be related to instrument (equipment) limitations or scientific limits.','USER','dbo','TABLE','RA_ANL_TOLERANCE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS MEASUREMENT TYPE: All possible names, codes and other identifiers for valid measurement types. ANL QC VALID MEASURE lists which measurement types are valid for various types of analysis, and what valid ranges for the values should be. In analysis detail tables, ensure that you have selected a measurement type that is appropriate for the type of study.','USER','dbo','TABLE','RA_ANL_VALID_MEASUREMENT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS VALID MEASUREMENT TYPE: All possible names, codes and other identifiers for valid measurement types. ANL QC VALID MEASURE lists which measurement types are valid for various types of analysis, and what valid ranges for the values should be. In analysis detail tables, ensure that you have selected a measurement type that is appropriate for the type of study.','USER','dbo','TABLE','RA_ANL_VALID_MEAS_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ANALYSIS FORMULA TYPE: All possible names, codes and other identifiers for the type of formula that has been described. A common kind of calcuation method or formula type are RATIOS.','USER','dbo','TABLE','RA_ANL_VALID_PROBLEM';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WATER ANALYSIS PROPERTY: All possible names, codes and other identifiers for the compositional and/or physical properties being analyzed during a water analysis.','USER','dbo','TABLE','RA_ANL_WATER_PROPERTY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ABSOLUTE OPEN FLOW: All possible names, codes and other identifiers for the type of Absolute Open Flow procedure. For example, Simplified or Lit procedure.','USER','dbo','TABLE','RA_AOF_ANALYSIS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ABSOLUTE OPEN FLOW CALCULATION METHOD: All possible names, codes and other identifiers for the type of method used to calculate the absolute open flow potential of the well. For example, single point, multi-point, theoretical or incomplete data.','USER','dbo','TABLE','RA_AOF_CALC_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS AMERICAN PETROLEUM INSTITUTE LOG SYSTEM: All possible names, codes and other identifiers for which API system was used.','USER','dbo','TABLE','RA_API_LOG_SYSTEM';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS APPLICATION COMPONENT TYPE: All possible names, codes and other identifiers for the type of component associated with the application.','USER','dbo','TABLE','RA_APPLICATION_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS APPLICATION ATTACHMENT TYPE: All possible names, codes and other identifiers for the type of appliation attachment that has been sent, such as maps, reports, letters, contracts and so on.','USER','dbo','TABLE','RA_APPLIC_ATTACHMENT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS APPLICATION BUSINESS ASSOCIATE ROLE: All possible names, codes and other identifiers for the role that a business associate played in the application (approver, creator, reviewer etc.).','USER','dbo','TABLE','RA_APPLIC_BA_ROLE';
go

execute sp_addextendedproperty 'Description','LAND RIGHT APPLICATION DECISION: All possible names, codes and other identifiers for the decision on the applicaiton, such as approved, denied etc.','USER','dbo','TABLE','RA_APPLIC_DECISION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS APPLICATION DESC: All possible names, codes and other identifiers for the type of descriptive information provided with an application, such as start date, end date, camp location, crew size, equipment type etc.','USER','dbo','TABLE','RA_APPLIC_DESC';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS APPLICATION REMARK TYPE: All possible names, codes and other identifiers for the type of remark about the applicaiton, such as decision remark.','USER','dbo','TABLE','RA_APPLIC_REMARK_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE APPLICATION STATUS: All possible names, codes and other identifiers for the status of the application, such as pending, approved, waiting on documents etc.','USER','dbo','TABLE','RA_APPLIC_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS APPLICATION TYPE: All possible names, codes and other identifiers for the type of application being made, such as application to drill, application to extend a land right, application to conduct geophysical operations etc. Examples: continuation, groupings, license validations, offset notice appeal, selections, grouping, continuation, significant discovery area, significant discovery license, expiry notification, commercial discovery area, production license.','USER','dbo','TABLE','RA_APPLIC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS AREA COMPONENT TYPE: All possible names, codes and other identifiers for the type of component associated with an area.','USER','dbo','TABLE','RA_AREA_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE AREA CONTAIN TYPE: All possible names, codes and other identifiers for a reference to the type of containment, such as a full legal containment, a partial containment (or overlap).','USER','dbo','TABLE','RA_AREA_CONTAIN_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE AREA DESCRIPTION CODE: All possible names, codes and other identifiers for a codified description of an area, such as a project area.','USER','dbo','TABLE','RA_AREA_DESC_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE AREA DESCRIPTION TYPE: All possible names, codes and other identifiers for The type of description of an area, such as size, terrain, vegetation etc.','USER','dbo','TABLE','RA_AREA_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS AREA TYPE: All possible names, codes and other identifiers for the type of area described, such as country, province, basin, project, business area etc.','USER','dbo','TABLE','RA_AREA_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS AREA CROSS REFERENCE TYPE: All possible names, codes and other identifiers of valid reasons for relating areas to each other. These may refer to organizations, jurisdictional relationships etc.','USER','dbo','TABLE','RA_AREA_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS AUTHORITY TYPE: All possible names, codes and other identifiers for the type of authority given to a business associate, often an employee of a company. Authority may be extended for purchase authorizations, to sign contracts or to enter into negotiations etc.','USER','dbo','TABLE','RA_AUTHORITY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS AUTHOR TYPE: All possible names, codes and other identifiers for the type of author of a document or other product. Could be who the product was created for, the company that created it, the person who created it, the scientist who was in charge etc.','USER','dbo','TABLE','RA_AUTHOR_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALISA BUSINESS AUTHORITY COMPONENT TYPE: All possible names, codes and other identifiers for the type of component associated with the business authority.','USER','dbo','TABLE','RA_BA_AUTHORITY_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BA CATEGORY: All possible names, codes and other identifiers for the category that the business associate is in. For a company, may be legal company, sole proprietorship, corporation etc.','USER','dbo','TABLE','RA_BA_CATEGORY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BUSINESS ASSOCIATE COMPONENT TYPE: All possible names, codes and other identifiers for the type of component associated with a business associate.','USER','dbo','TABLE','RA_BA_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALISA BA CONTACT LOCATION TYPE: All possible names, codes and other identifiers for the type of contact location defined. May be phone number, fax number, Email address, Web URL etc.','USER','dbo','TABLE','RA_BA_CONTACT_LOC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CREW OVERHEAD TYPE: All possible names, codes and other identifiers for the type of overhead paid to a crew member during a peiod, such as cost of living allowance.','USER','dbo','TABLE','RA_BA_CREW_OVERHEAD_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CREW TYPE: All possible names, codes and other identifiers for valid kinds of crews, such as drilling crews, cleanup crews, inspection crews, logging crews or seismic crews.','USER','dbo','TABLE','RA_BA_CREW_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BA DESCRIPTION DETAIL CODE: All possible names, codes and other identifiers for the case that the detail is described as a coded value, this table provides the list of valid codes for each type of detail.','USER','dbo','TABLE','RA_BA_DESC_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BA REFERENCE VALUE TYPE: All possible names, codes and other identifiers for the case where a detail is referenced to some other value the type of reference value is captured here.','USER','dbo','TABLE','RA_BA_DESC_REF_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BA DESCRIPTION DETAIL TYPE: All possible names, codes and other identifiers for the kind of detail information about the business associate that has been stored.','USER','dbo','TABLE','RA_BA_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BA LICENSE DUE CONDITION: All possible names, codes and other identifiers for the state that must be achieved for the condition to become effective. For example, a report may be due 60 days after operations commence (or cease).','USER','dbo','TABLE','RA_BA_LIC_DUE_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BA LICENSE VIOLATION TYPE: All possible names, codes and other identifiers for the type of violation of a license that is being recorded. Can be as simple as failure to submit necessary reports or something more difficult such as improper procedures.','USER','dbo','TABLE','RA_BA_LIC_VIOLATION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BA LICENSE VIOLATION RESOLUTION TYPE: All possible names, codes and other identifiers for the type of resolution to a violation of a license term, such as the payment of a fine or creation of new processes.','USER','dbo','TABLE','RA_BA_LIC_VIOL_RESOL';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BUSINESS ASSOCIATE ORGANIZATION COMPONENT TYPE: All possible names, codes and other identifiers for the type of component associated with a business associate organization.','USER','dbo','TABLE','RA_BA_ORGANIZATION_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BA ORGANIZATION TYPE: All possible names, codes and other identifiers for the Business Associate Organization type. This may be department, division, cost center, business unit, franchise etc.','USER','dbo','TABLE','RA_BA_ORGANIZATION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BUSINSS ASSOCIATE PERMIT TYPE: All possible names, codes and other identifiers for the type of permit that the business associate has, such as drilling, seismic exploration etc.','USER','dbo','TABLE','RA_BA_PERMIT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BA PREFERENCE TYPE: All possible names, codes and other identifiers for the type of preference documented, such as preference for meeting times, well log curve selection, parameter useage etc.','USER','dbo','TABLE','RA_BA_PREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BUSINESS ASSOCIATE SERVICE TYPE: All possible names, codes and other identifiers for services for a business associate. For example well logger, drilling contractor, application developer. For land, may be may be address for service, brokerage, maintainor etc.','USER','dbo','TABLE','RA_BA_SERVICE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE BA STATUS: All possible names, codes and other identifiers for the current status of the Business Associate, such as Active, In Receivership, Sold, Merged.','USER','dbo','TABLE','RA_BA_STATUS';
go

execute sp_addextendedproperty 'Description','BA TYPE: All possible names, codes and other identifiers for the type of business associate. Usual reference values include COMPANY, PERSON, REGULATORY, SOCIETY, ASSOCIATION.','USER','dbo','TABLE','RA_BA_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BA XREF TYPE: All possible names, codes and other identifiers for the Business Associate cross-reference Type. May be buy-out, name change, merger etc. NOT to be used for the organizational structure, or to track employee/employer relationships (this goes in BA organization).','USER','dbo','TABLE','RA_BA_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BHP METHOD: All possible names, codes and other identifiers for the method of measuring the bottom hole pressure (e.g., measured, calculated, etc.).','USER','dbo','TABLE','RA_BHP_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BOTTOM HOLE PRESSURE TEST TYPE: All possible names, codes and other identifiers for the type of bottom hole pressure test conducted on the wellbore. For example, bottom hole static gradient, bottom hole buildup, top hole buildup etc.','USER','dbo','TABLE','RA_BH_PRESS_TEST_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DRILL BIT BEARING CONDITION: All possible names, codes and other identifiers for the condition of the drill bit bearing when it is pulled from the hole, such as worn, broken etc.','USER','dbo','TABLE','RA_BIT_BEARING_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BIT CUTTING STRUCTURE MAJOR DULL CHARACTERISTIC: All possible names, codes and other identifiers for IADC Roller Bit Dull Grading major dull characteristics of bit such as BC Broken Cone, LN Lost Nozzle, BT Broken teeth/cutters, LT Lost Teeth/Cutters, BU Balled Up, NO No Major/Other Dull Characteristics, CC Cracked Cone (show cone numbers under location) etc.','USER','dbo','TABLE','RA_BIT_CUT_STRUCT_DULL';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DRILL BIT CUTTING STRUCTURE INNER: All possible names, codes and other identifiers for IADC Roller Bit Dull Grading inner 2/3 of bit cutting structure tooth condition. Valid values 0-8 in the IADC standard.','USER','dbo','TABLE','RA_BIT_CUT_STRUCT_INNER';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CUTTING STRUCTURE LOCATION: All possible names, codes and other identifiers for the IADC Roller Bit Dull Grading location of cracked or dragged cones. All Rows, Heel Rows, Middle Rows, Nose Rows.','USER','dbo','TABLE','RA_BIT_CUT_STRUCT_LOC';
go

execute sp_addextendedproperty 'Description','DRILL BIT CUTTING STRUCTURE OUTER: All possible names, codes and other identifiers for the condition of the outer 1/2 of the tooth, derived from the IADC Roller Bit Dull Grading and outer 1/3 of bit cutting structure tooth condition. Valid values 0-8 inthe IADC standard.','USER','dbo','TABLE','RA_BIT_CUT_STRUCT_OUTER';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DRILL BIT REASON PULLED: All possible names, codes and other identifiers for IADC Roller Bit Dull Grading and reason dull bit pulled such as BHA CHG Bottom Hole Assembly, LOG Run Logs, CD Condition Mud PP Pump Pressure, CP Core Point, PR Penetration Rate.','USER','dbo','TABLE','RA_BIT_REASON_PULLED';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BLOWOUT FLUID: All possible names, codes and other identifiers for the type of fluid blown out of a well when a high pressure zone is encountered. For example gas, oil or water.','USER','dbo','TABLE','RA_BLOWOUT_FLUID';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BUILDUP RADIUS: All possible names, codes and other identifiers for the magnitude of the buildup radius for the horizontal well. For example, the types of buildup radius can be long, medium or short.','USER','dbo','TABLE','RA_BUILDUP_RADIUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CATALOGUE ADDITIVE GROUP: All possible names, codes and other identifiers for the class or group of additives that this additive belongs to, such as drill mud additive, treatment additive, processing additive etc. Within each group of additives, many types of additives may be described using CAT ADDITIVE TYPE.','USER','dbo','TABLE','RA_CAT_ADDITIVE_GROUP';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CATALOGUE ADDITIVE QUANTITY: All possible names, codes and other identifiers for the type of quantity in which this particular additive is available, such as sacks, pallets, bales, killograms etc.','USER','dbo','TABLE','RA_CAT_ADDITIVE_QUANTITY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CATALOGUE ADDITIVE SPECIFICATION TYPE: All possible names, codes and other identifiers for the kinds of specifications that may be defined for an additive, such as the volume added, weight added, mixing method, preparaation method etc.','USER','dbo','TABLE','RA_CAT_ADDITIVE_SPEC';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ADDITIVE CATALOGUE CROSS REFERENCE TYPE: All possible names, codes and other identifiers for the kind of relationship between additives. For example, a new additive may be developed to replace an older product, or two products may be equivalent.','USER','dbo','TABLE','RA_CAT_ADDITIVE_XREF';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CATALOGUE EQUIPMENT GROUP: All possible names, codes and other identifiers for the functional group of equipment types, such as vehicles, drilling rigs, measuring equipment, monitoring equipment etc. Note that the function of this table may also be assumed by the CLASSIFICATION module for more robust and complete classifications.','USER','dbo','TABLE','RA_CAT_EQUIP_GROUP';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS EQUIPMENT CATALOGUE SPECIFICATION TYPE: All possible names, codes and other identifiers for the type of specification, such as diameter, strength, length, resonating frequency etc. that are listed in the general specifications for a kind of equipment.','USER','dbo','TABLE','RA_CAT_EQUIP_SPEC';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SPECIFICATION TYPE: All possible names, codes and other identifiers for the type of specification, such as diameter, strength, length, resonating frequency etc.','USER','dbo','TABLE','RA_CAT_EQUIP_SPEC_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CATALOGUE EQUIPMENT TYPE: All possible names, codes and other identifiers for the type of equipment that is listed, can be grouped into broad classifications with R CAT EQUIP GROUP if you wish. Note that the function of this table may also beassumed by the CLASSIFICATION module for more robust and complete classifications.','USER','dbo','TABLE','RA_CAT_EQUIP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CEMENT TYPE: All possible names, codes and other identifiers for identifying the particular type of cement (and additive) used during a cementing operation.','USER','dbo','TABLE','RA_CEMENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CHECKSHOT SURVEY TYPE: Use this table to record all possible names, codes and other identifiers for the type of checkshot survey that was conducted to acquire this data, such as VSP, inline checkshot, walkaway checkshot etc.','USER','dbo','TABLE','RA_CHECKSHOT_SRVY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CLASSIFICATION DESCRIPTION PROPERTIES: Use this table to record all possible names, codes and other identifiers for the kinds of properties that define levels in a classification system, and also defines how the properties are to be described in CLASS LEVEL DESC.','USER','dbo','TABLE','RA_CLASS_DESC_PROPERTY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CLASSIFICATION LEVEL COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the relationships between specific levels of the classification systems and busines objects, such as wells, equipment, documents, seismic sets and land rights. You can also use Classification Systems to embed hierarchies into reference tables, by indicating the name of the reference table that has been classified. In this case, the values in the Classification system should correspond to the values in the reference table (see CLASS LEVEL ALIAS).','USER','dbo','TABLE','RA_CLASS_LEV_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CLASSIFICATION SYSTEM CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the types of valid relationships between levels of a classification system, such as to establish overlap or equivalence in content, or to indicate the parent(s) of a level.','USER','dbo','TABLE','RA_CLASS_LEV_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CLASS SYSTEM DIMENSION: Use this table to record all possible names, codes and other identifiers for the type of dimension or facet that is in this taxomony or classification system. For example, a taxonomy may exist for an organization, or for geographic areas, or for tools and equipment or materials. By prefrence, taxonomies should contain one dimension or as few dimensions as possible. For classification purposes, each business object can refer to as many classification systems as necessary.','USER','dbo','TABLE','RA_CLASS_SYSTEM_DIMENSION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CLASSIFICATION SYSTEM CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the types of relationships between classification systems. For example, you may indicate that a classification system is approximately the same, or that one is a newer version of another.','USER','dbo','TABLE','RA_CLASS_SYST_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CLIMATE TYPE: Use this table to record all possible names, codes and other identifiers for the valid types of climate, such as arctic, temperate.','USER','dbo','TABLE','RA_CLIMATE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS COAL RANK SCHEME TYPE: Use this table to record all possible names, codes and other identifiers for the type of coal rank scheme that is referenced. Could be a formal, recognized scheme, a working scheme etc.','USER','dbo','TABLE','RA_COAL_RANK_SCHEME_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CODE VERSION CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of relationship between two reference values in a table, such as equivalent meaning, replacement value, is a kind of and so on.','USER','dbo','TABLE','RA_CODE_VERSION_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS COLLAR TYPE: Use this table to record all possible names, codes and other identifiers for the type of collar used to couple the tubular with another tubing string.','USER','dbo','TABLE','RA_COLLAR_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS COLOR: Use this table to record all possible names, codes and other identifiers for valid colors.','USER','dbo','TABLE','RA_COLOR';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS COLOR EQUIVALENT: - Use this table to record all possible names, codes and other identifiers for equivalent colors in different palettes.','USER','dbo','TABLE','RA_COLOR_EQUIV';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS COLOR FORMAT: Use this table to record all possible names, codes and other identifiers for the type of color format that has been used, For digital files. May be expressed as a name (monochrome, greyscale, color) or as a bit value (the number of bits used to reprsent a single pixel of the image. Bi-tonal images have one bit per pixel, 1 BPP. Often RGB images use 24 BPP).','USER','dbo','TABLE','RA_COLOR_FORMAT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS COLOR PALETTE: Use this table to record all possible names, codes and other identifiers for the palette that defines the set of colors in use. Palettes include web safe palettes (216 colors), pantone colors (used for inks) etc.','USER','dbo','TABLE','RA_COLOR_PALETTE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL COMPLETION METHOD: Use this table to record all possible names, codes and other identifiers for the type of aperature through which the fluid entered into the well tubing.','USER','dbo','TABLE','RA_COMPLETION_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS COMPLETION STATUS: Use this table to record all possible names, codes and other identifiers for the type of completion or perforation status. For example, the status can be open, closed, squeezed, plugged, etc.','USER','dbo','TABLE','RA_COMPLETION_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS COMPLETION STATUS TYPE: Use this table to record all possible names, codes and other identifiers for the group or type of status, such as construction, financial, legal etc.','USER','dbo','TABLE','RA_COMPLETION_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS COMPLETION TYPE: Use this table to record all possible names, codes and other identifiers for the types of well completions or methods. For example perforation, open hole, gravel pack or combination.','USER','dbo','TABLE','RA_COMPLETION_TYPE';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_CONDITION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONFIDENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of confidence that is associated with this value. For biostratigraphic analysis, could be confidence in any of the values provided such asthe species identification, the diversity count etc.','USER','dbo','TABLE','RA_CONFIDENCE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONFEDENTIALITY REASON: Use this table to record all possible names, codes and other identifiers for the reason why information or records are confidential, such as legislated confidentiality period, corporate security etc.','USER','dbo','TABLE','RA_CONFIDENTIAL_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONFIDENTIALITY TYPE: Use this table to record all possible names, codes and other identifiers for the types of confidentiality types usually associated with a well. For example confidential, non-confidential or confidential 90 days.','USER','dbo','TABLE','RA_CONFIDENTIAL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONFORMITY RELATION: Use this table to record all possible names, codes and other identifiers for the type of conformity relationship that describes the surface that was picked. May be unconformity, disconformity, angular unconformity,conformable ro paraconformable. An unconformity is a substantial break or gap in the geologic record wher a rock unit is overlain by another that is not next in stratigraphic succession. Normally implies uplift or erosion with loss of the previously formed strata.','USER','dbo','TABLE','RA_CONFORMITY_RELATION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONSENT BUSINESS ASSOCIATE ROLE: Use this table to record all possible names, codes and other identifiers for the role played by a business associate in obtaining a consent, such as signing authority, chief negotiator etc.','USER','dbo','TABLE','RA_CONSENT_BA_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONSENT COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a consent.','USER','dbo','TABLE','RA_CONSENT_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONSENT CONDITION: Use this table to record all possible names, codes and other identifiers for a condition that has been imposed as a result of the consent granted. Each condition is based on the condition type, so that a set of conditions for road access may be kept seperate from conditions for dock usage.','USER','dbo','TABLE','RA_CONSENT_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONSENT REMARK TYPE: Use this table to record all possible names, codes and other identifiers for a code classifying the remark or type of remark.','USER','dbo','TABLE','RA_CONSENT_REMARK';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CURRENT CONSENT STATUS: Use this table to record all possible names, codes and other identifiers for the current status of this consent such as approved, pending, denied, waiting for report etc.','USER','dbo','TABLE','RA_CONSENT_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONSENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of consent sought, such as proximity consent, crossing consent, trapper consent, road use agreement.','USER','dbo','TABLE','RA_CONSENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONSULTATION ATTENDANCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of attendance at a discussion, such as regrets, in person, by phone connection, represented in written document etc.','USER','dbo','TABLE','RA_CONSULT_ATTEND_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONSULTATION COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component that is associated with the consultation. Could be a land right, seismic set, contract, facility etc.','USER','dbo','TABLE','RA_CONSULT_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONSULTATION DISCUSSION TYPE: Use this table to record all possible names, codes and other identifiers for the nominal type of discussion that was held, such as phone, mail, email, chat or in person.','USER','dbo','TABLE','RA_CONSULT_DISC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONSULTATION ISSUE TYPE: Use this table to record all possible names, codes and other identifiers for valid consultation detail types. Details may include the issues that are raised or resolved etc.','USER','dbo','TABLE','RA_CONSULT_ISSUE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONSULTATION REASON: Use this table to record all possible names, codes and other identifiers for the reason the consultation has been undertaken. Could be to obtain compliance with a specific regulation or to negotiate a contract etc.','USER','dbo','TABLE','RA_CONSULT_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONSULTATION RESOLUTION: Use this table to record all possible names, codes and other identifiers for a valid type of resolution to an issue raised in consultation, such as built fence, purchase equipment, provide training.','USER','dbo','TABLE','RA_CONSULT_RESOLUTION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONSULTATION BA ROLE: Use this table to record all possible names, codes and other identifiers for valid roles that can be played by participants in a consultation. Examples include counsil, observer, initiator etc.','USER','dbo','TABLE','RA_CONSULT_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONSULTATION TYPE: Use this table to record all possible names, codes and other identifiers for valid consultation types that are undertaken. Could be for negotiating a benefits agreement, obtaining surface access, use of a support facility etc.','USER','dbo','TABLE','RA_CONSULT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONSULTATION CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of consultation relationship that exists. For example, a consultation may be a component of a larger consultation project, or can replace or supplement another consultation.','USER','dbo','TABLE','RA_CONSULT_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BA INTEREST SET PARTNER CONTACT ROLE: Use this table to record all possible names, codes and other identifiers for the role played by the contact for the partner in the interest set, such as negotiator, authorization, legal representative etc.','USER','dbo','TABLE','RA_CONTACT_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONTAMINANT TYPE: Use this table to record all possible names, codes and other identifiers for the type of contaminant that may be present in a well test recovery or sample analysis. For example corrosive gases.','USER','dbo','TABLE','RA_CONTAMINANT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONTEST COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a contest.','USER','dbo','TABLE','RA_CONTEST_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONTEST PARTY ROLE: Use this table to record all possible names, codes and other identifiers for the role the party played in the contest, such as mediator, plaintiff, defendant, arbitrator etc.','USER','dbo','TABLE','RA_CONTEST_PARTY_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONTEST RESOLUTION METHOD: Use this table to record all possible names, codes and other identifiers for the method used to arrive at the resolution of the land right contest, such as binding arbitration, court ruling, mutual accord etc.','USER','dbo','TABLE','RA_CONTEST_RESOLUTION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONTEST TYPE: Use this table to record all possible names, codes and other identifiers for the type of contest oversuch as a land ownership or rights dispute.','USER','dbo','TABLE','RA_CONTEST_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONTRACT COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a contract.','USER','dbo','TABLE','RA_CONTRACT_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONTRACT BUSINESS ASSOCIATE ROLE: Use this table to record all possible names, codes and other identifiers for the role that is played by a business associate for the support of a contract.','USER','dbo','TABLE','RA_CONT_BA_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONTRACT COMPONENT REASON TYPE: Use this table to record all possible names, codes and other identifiers for the reason why the component is associated with the contract, such as acquired under terms of the contract, governed by the contract,part of litigation process etc.','USER','dbo','TABLE','RA_CONT_COMP_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS EXTENSION CONDITION: Use this table to record all possible names, codes and other identifiers for the method by which the contract may be managed or extended over its life time. For example, a contract may be held by production, held for the life of the lease, evergreen (goes year to year until one party terminates) or must be renegotiated at the end of the primary term. In some cases, specific conditions must be met for the contract to extend past the primary term.','USER','dbo','TABLE','RA_CONT_EXTEND_COND';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONTRACT EXTENSION TYPE: Use this table to record all possible names, codes and other identifiers for the type of extension that has been granted for the contract. May be based on production status, statute, contract conditions etc.','USER','dbo','TABLE','RA_CONT_EXTEND_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INSURANCE ELECTION: All parties of the contract agree that they are self insured, and additional coverage is not necessary. This means that if there is an actionable problem during operations, the operator may be required to pay own legal costs without recourse to reimbursement. Could also be that the Operator is insured. Use this table to record all possible names, codes and other identifiers for the level of elected insurance.','USER','dbo','TABLE','RA_CONT_INSUR_ELECT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS OPERATING PROCEDURE CODE: Use this table to record all possible names, codes and other identifiers for the version of a standard operating procedure that you are using.','USER','dbo','TABLE','RA_CONT_OPERATING_PROC';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONTRACT PROVISION TYPE: Use this table to record all possible names, codes and other identifiers for values for types of contract provisions (e.g. EARNING, POOLED INTERESTS, etc.)','USER','dbo','TABLE','RA_CONT_PROVISION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONTRACT PROVISION CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the types of relationships between contract provisions, such as when a stipulation in one contract overrides another stipulation, or to refer to other relevant information. (e.g. a royalty agreement which stipulates who the royalty is paid to but the parties who pay the royalty change by virtue of a joint operating agreement).','USER','dbo','TABLE','RA_CONT_PROV_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONTRACT STATUS: Use this table to record all possible names, codes and other identifiers for valid status types for a contract, such as active, inactive, pending, terminated, draft etc.','USER','dbo','TABLE','RA_CONT_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONTRACT STATUS TYPE: Use this table to record all possible names, codes and other identifiers for valid status types for a contract, such as legal status, negotiation status, financial status etc.','USER','dbo','TABLE','RA_CONT_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONTRACT TYPE: Use this table to record all possible names, codes and other identifiers for valid types of contract, such as pooling agreement, joint venture, joint operating agreement, farm-out.','USER','dbo','TABLE','RA_CONT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONTRACT VOTING RESPONSE: Use this table to record all possible names, codes and other identifiers for the types of response allowed for a vote. Usually three responses (abstain, for, against). Alternate terms may be used, such as yes / no,positive / negative, agree / disagree etc.','USER','dbo','TABLE','RA_CONT_VOTE_RESPONSE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONTRACT VOTING PROCEDURE TYPE: Use this table to record all possible names, codes and other identifiers for the type of voting procedure that is captured, such as general operations, enlargment, exhibits.','USER','dbo','TABLE','RA_CONT_VOTE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CONTRACT CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of relationship between two contracts, such as supercedence or governing relationship. Note that relationships between contract provisions is captured in the table CONT PROVISION XREF.','USER','dbo','TABLE','RA_CONT_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE COORDINATE CAPTURE METHOD: Use this table to record all possible names, codes and other identifiers for valid methods of capturing coodinate data. For example: Digitizing, Surveying, etc.','USER','dbo','TABLE','RA_COORD_CAPTURE';
go

execute sp_addextendedproperty 'Description','REFERENCE COORDINATE COMPUTATION METHOD: Use this table to record all possible names, codes and other identifiers for valid methods of computing coordinate values. For example: ATS21 (using bilinear interpolation and the Alberta Township System Version 2.1 grid nodes.)','USER','dbo','TABLE','RA_COORD_COMPUTE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS COORDINATE QUALITY: Use this table to record all possible names, codes and other identifiers for the quality of the coordiante, such as validated, unvalidated, poor etc.','USER','dbo','TABLE','RA_COORD_QUALITY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS COORDINATE SYSTEM TYPE: Use this table to record all possible names, codes and other identifiers for the type of coordinate system. Will include Geographic coordinate system, local spatial coordinate system, Geocentric coordinate system, Map Grid coordinate system, and vertical coordinate system.','USER','dbo','TABLE','RA_COORD_SYSTEM_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CORE HANDLING: Use this table to record all possible names, codes and other identifiers for the type of technique used to preserve the core. For example, wrapped in plastic or fibreglass sleeve.','USER','dbo','TABLE','RA_CORE_HANDLING';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CORE RECOVERY TYPE: Use this table to record all possible names, codes and other identifiers for the type of core recovery. For sidewall cores the values may be recovered, lost or misfired.','USER','dbo','TABLE','RA_CORE_RECOVERY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS CORE SAMPLE: Use this table to record all possible names, codes and other identifiers for the type of core sample. The core sample may be a full diameter (whole core) or a plug sample (button, plug, cutting).','USER','dbo','TABLE','RA_CORE_SAMPLE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CORE SHIFT METHOD: Use this table to record all possible names, codes and other identifiers for the method used to correct core depths to adjusted wireline log depths.','USER','dbo','TABLE','RA_CORE_SHIFT_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CORE SOLVENT: Use this table to record all possible names, codes and other identifiers for the solvent used for removing residual fluids from the core. For example, a common fluid used for distillation-extraction is toluene.','USER','dbo','TABLE','RA_CORE_SOLVENT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CORE TYPE: Use this table to record all possible names, codes and other identifiers for the type of core procedure used during the coring operation. For example, conventional, sidewall, diamond, triangle etc..','USER','dbo','TABLE','RA_CORE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CORRECTION METHOD: Use this table to record all possible names, codes and other identifiers for the correction method used to repair damage done to a physical item.','USER','dbo','TABLE','RA_CORRECTION_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS COUPLING TYPE: Use this table to record all possible names, codes and other identifiers for a short length of pipe used to connect two joints of casing. A casing coupling has internal threads (female threadform) machined to match the external threads (male threadform) of the long joints of casing. The two joints of casing are threaded into opposite ends of the casing coupling. Synonyms: casing collar (Schlumberger Oilfield Glossary)','USER','dbo','TABLE','RA_COUPLING_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CREATOR TYPE: Use this table to record all possible names, codes and other identifiers for the type of creatorship of a document, report or other object. Could be primary author, corporate author, scientific author, laboratory, field collectionetc.','USER','dbo','TABLE','RA_CREATOR_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TRANSFORM PARAMETERS: Use this table to record all possible names, codes and other identifiers for a valid transform parameter that may be applied during a conversion between coordinate systems.','USER','dbo','TABLE','RA_CS_TRANSFORM_PARM';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TRANSFORM TYPE: Use this table to record all possible names, codes and other identifiers for identifying valid Geodetic Transformation types. For example, Bursa-Wolfe, Molodensky, Cartesian, Geocentric or Grid.','USER','dbo','TABLE','RA_CS_TRANSFORM_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CURVE SCALE: Use this table to record all possible names, codes and other identifiers for the type of curve scale. For example, the valid codes may be straight, shift, X5 or X10.','USER','dbo','TABLE','RA_CURVE_SCALE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LOG CURVE TYPE: Use this table to record all possible names, codes and other identifiers for the type of wireline log curve recorded during the logging operation. For example, caliper, gamma ray.','USER','dbo','TABLE','RA_CURVE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LOG CURVE CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for The level of the cross reference that is captured. As this is a breakout table, you have the option of capturing each level in a hierarchyexplicitly, so that relationships may be parent to child, grandparent to child, great grandparent to child etc. Other types of relationships may also be defined here.','USER','dbo','TABLE','RA_CURVE_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CUSHION TYPE: Use this table to record all possible names, codes and other identifiers for the type of cushion used during a well test. For example water, nitrogen, ammonia, or carbon dioxide.','USER','dbo','TABLE','RA_CUSHION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CUTTING FLUID: Use this table to record all possible names, codes and other identifiers for the type of fluid used to cut the core into samples.','USER','dbo','TABLE','RA_CUTTING_FLUID';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PROCESS DONE: Use this table to record all possible names, codes and other identifiers for a records managment process, such as pulled, shipped etc.','USER','dbo','TABLE','RA_DATA_CIRC_PROCESS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DATA CIRCULATION STATUS: Use this table to record all possible names, codes and other identifiers for the status of the item, such as checked in or out.','USER','dbo','TABLE','RA_DATA_CIRC_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DATA STORE TYPE: Use this table to record all possible names, codes and other identifiers for the type of data store that is referenced, such as disk, folder, tape, shelf, SAN system server or optical disk.','USER','dbo','TABLE','RA_DATA_STORE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DATE FORMAT TYPE: Use this table to record all possible names, codes and other identifiers for the type of date format used in this table, such as YYYY or YYYYQQ or YYYYMM or YYYYMMDD etc. Indicates the degree of accuracy in the dates.','USER','dbo','TABLE','RA_DATE_FORMAT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS GEODETIC DATUM ORIGIN: Use this table to record all possible names, codes and other identifiers for the valid origins for Geodetic Datums. For example, Geocentric, Local Origin, Local Meridian.','USER','dbo','TABLE','RA_DATUM_ORIGIN';
go

execute sp_addextendedproperty 'Description','PRODUCTION DECLINE CURVE CONDITION CODE: Use this table to record all possible names, codes and other identifiers for a validated set of codes that may be associated with certain types of decline condition types. Note that only some condition types will have codes. Others will be associated with numberic or text descriptions only. Codes may be used to indicate whether the number of wells includes producing wells, injecting wells or both for example.','USER','dbo','TABLE','RA_DECLINE_COND_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PRODUCTION DECLINE CURVE CONDITION TYPE: Use this table to record all possible names, codes and other identifiers for the type of condition that is described for the production decline analysis, such as the number of producing oil wells, number of injection wells, service factors etc.','USER','dbo','TABLE','RA_DECLINE_COND_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DECLINE CURVE TYPE: Use this table to record all possible names, codes and other identifiers for the type of decline curve that is used in decline curve forecast calculations such as exponential, harmonic, hyperbolic, linear, etc.','USER','dbo','TABLE','RA_DECLINE_CURVE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DECLINE TYPE: Use this table to record all possible names, codes and other identifiers for the type of decline that is used in decline curve forecast calculations such as nominal or effective percentatge.','USER','dbo','TABLE','RA_DECLINE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DECRYPTION TYPE: Use this table to record all possible names, codes and other identifiers for the type of decryption that must be done to this file to get at the data. Examples include unzip, untar, run a specified procedure etc.','USER','dbo','TABLE','RA_DECRYPT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DEDUCTION TYPE: Use this table to record all possible names, codes and other identifiers for the type of decution to be made to a payment, such as tax, CPP, provincial tax, state tax, federal tax.','USER','dbo','TABLE','RA_DEDUCT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DIGITAL FORMAT: Use this table to record all possible names, codes and other identifiers for the digital format, or predefined order or arrangement of digits. For trace data, may be SEG Y or SEG B, for survey data may be UKOOA or SEG P1 etc.','USER','dbo','TABLE','RA_DIGITAL_FORMAT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DIGITAL OUTPUT: Use this table to record all possible names, codes and other identifiers for the format that a parameter is to be output as when reporting or recreating a digital file.','USER','dbo','TABLE','RA_DIGITAL_OUTPUT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DIRECTION: Use this table to record all possible names, codes and other identifiers for a set of valid compass directions, used for referencing positional information. For example, N, S, NE etc.','USER','dbo','TABLE','RA_DIRECTION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TABLE DIRECTIONAL SURVEY ACCURACY REASON: Use this table to record all possible names, codes and other identifiers for the reasons why station accuracy may be affected in a directional survey.','USER','dbo','TABLE','RA_DIR_SRVY_ACC_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DIRECTIONAL SURVEY CLASS: Use this table to record all possible names, codes and other identifiers for valid classes of directional surveys. For example, directional survey where both inclination and azimuth measured, hole deviation where only inclination measured.','USER','dbo','TABLE','RA_DIR_SRVY_CLASS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DIRECTIONAL SURVEY PROCESS METHOD: The processing method of the reported (original hardcopy, PDF, etc.) data: interpolated, non-interpolated or mixed...','USER','dbo','TABLE','RA_DIR_SRVY_COMPUTE';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_DIR_SRVY_CORR_ANGLE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DIRECTIONAL SURVEY POINT TYPE: Use this table to record all possible names, codes and other identifiers for the valid ways survey measurements are obtained for a directional survey observation point. For example, measured, extrapolated beyond a survey, interpolated between t wo existing survey points.','USER','dbo','TABLE','RA_DIR_SRVY_POINT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DIRECTIONAL SURVEY PROCESS METHOD: The processing method of the reported (original hardcopy, PDF, etc.) data: interpolated, non-interpolated or mixed.','USER','dbo','TABLE','RA_DIR_SRVY_PROCESS_METH';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TABLE DIRECTIONAL SURVEY RADIUS OF UNCERTAINTY: Use this table to record all possible names, codes and other identifiers for reasons and valid values related to radius of uncertainty.','USER','dbo','TABLE','RA_DIR_SRVY_RAD_UNCERT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DIRECTIONAL SURVEY RECORD MODE: Use this table to record all possible names, codes and other identifiers for valid record modes for Directional Surveys. For example, multi shot, single shot.','USER','dbo','TABLE','RA_DIR_SRVY_RECORD';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_DIR_SRVY_REPORT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DIRECTIONAL SURVEY TYPE: Use this table to record all possible names, codes and other identifiers for valid types of Directional Surveys. For example, gyroscopic, magnetic, MWD, hole deviation, totco, acid bottle, ...','USER','dbo','TABLE','RA_DIR_SRVY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DISTANCE REFERENCE POINT: Use this table to record all possible names, codes and other identifiers for the location name or reference point for measurement of distance to an object (e.g. offshore well) or point. Examples: Cape Fear, Port of Aberdeen, Jonesville.','USER','dbo','TABLE','RA_DIST_REF_PT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DOCUMENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of document. ie: monthly periodical or technical publication or specificiation. Could also be a report or analysis.','USER','dbo','TABLE','RA_DOCUMENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DOCUMENT STATUS: Use this table to record all possible names, codes and other identifiers for the status of the document. Can include whether the document has been executed, marked as draft etc.','USER','dbo','TABLE','RA_DOC_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DRILLING MEDIA: Use this table to record all possible names, codes and other identifiers for the various drilling media type present in a wellbore. Commonly refered to as MUD TYPE. For example chemical gel mud, crude oil or native mud.','USER','dbo','TABLE','RA_DRILLING_MEDIA';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DRILL ASSEMBLY COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component that has been placed on the assembly. Specific equipment parameters, manufacturer information etc may be captured by using the EQUIPMENT module.','USER','dbo','TABLE','RA_DRILL_ASSEMBLY_COMP';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DRILL BIT CONDITION: Use this table to record all possible names, codes and other identifiers for the condition of the drill bit when it is pulled from the hole, such as worn, broken etc.','USER','dbo','TABLE','RA_DRILL_BIT_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DRILL BIT DETAILCODE: Use this table to record all possible names, codes and other identifiers for the kinds of codes needed to describe each kind of detail record, in the case where the value captured in not numeric. For example, you may wish to track whether a sensor is ON or OFF.','USER','dbo','TABLE','RA_DRILL_BIT_DETAIL_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DRILL BIT DETAILTYPE: Use this table to record all possible names, codes and other identifiers for the bit and its condition or use that is not otherwise captured in the model.','USER','dbo','TABLE','RA_DRILL_BIT_DETAIL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DRILL BIT JET TYPE: Use this table to record all possible names, codes and other identifiers for the type of jet used for instance, Standard, Short Extended, etc.','USER','dbo','TABLE','RA_DRILL_BIT_JET_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DRILLING BIT TYPE: Use this table to record all possible names, codes and other identifiers for the type of drilling bit used to drill the wellbore segment.','USER','dbo','TABLE','RA_DRILL_BIT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DRILLING HOLE POSITION: Use this table to record all possible names, codes and other identifiers for the location on a vessel that describes the position of the drilling hole through which operations proceed.','USER','dbo','TABLE','RA_DRILL_HOLE_POSITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DRILL REPORT TIME: Use this table to record all possible names, codes and other identifiers for the valid points in a reporting period, shift or tour when measurements and records are made. Usually, this is done at the start or end of theshift.','USER','dbo','TABLE','RA_DRILL_REPORT_TIME';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DRILLING STATISTIC CODE: Use this table to record all possible names, codes and other identifiers for valid statistic or metrics values where the value is selected from a list. Each type of statistic may contain its own set of valid codes using this table.','USER','dbo','TABLE','RA_DRILL_STAT_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DRILLING STATISTIC TYPE: Use this table to record all possible names, codes and other identifiers for valid well operations or drilling statistics that are not explicitly defined in the PPDM data model. Use the PPDM PROPERTY SET and PPDM PROPERTY COLUMN tables to define how each kind of statistic should be captured.','USER','dbo','TABLE','RA_DRILL_STAT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DRILL TOOL TYPE: Use this table to record all possible names, codes and other identifiers for the types of drill tools. For example cable or rotary.','USER','dbo','TABLE','RA_DRILL_TOOL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ECONOMIC SCENARIO: Use this table to record all possible names, codes and other identifiers for the economic scenarios which have been set up to allow economics to be run under multiple pricing and operating cost assumptions (scenarios).','USER','dbo','TABLE','RA_ECONOMIC_SCENARIO';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ECONOMIC SCHEDULE: Use this table to record all possible names, codes and other identifiers for the types of values that are described in the economics run. Future versions of the model may re-engineer this table to support additional functionality.','USER','dbo','TABLE','RA_ECONOMIC_SCHEDULE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ECOZONE HIERARCHY LEVEL: Use this table to record all possible names, codes and other identifiers for the relationship between parent and child: parent child, grandparent child (two levels apart), great grandparent (3 levels apart) etc. Used for implemnetations who choose to populate all levels of a hierarchy explicitly and avoid the need to query using connect by syntax.','USER','dbo','TABLE','RA_ECOZONE_HIER_LEVEL';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ECOZONE TYPE: Use this table to record all possible names, codes and other identifiers for the type of ecozone that has been referenced, such as marine, terrestrial, lake atmospheric etc.','USER','dbo','TABLE','RA_ECOZONE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ECOZONE CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of cross reference between ecozones, such as superceded, replacement etc.','USER','dbo','TABLE','RA_ECOZONE_XREF';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS EMPLOYEE POSITION: Use this table to record all possible names, codes and other identifiers for valid types of employee positions. This list may come from human resource departments.','USER','dbo','TABLE','RA_EMPLOYEE_POSITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS EMPLOYEE STATUS: Use this table to record all possible names, codes and other identifiers for valid values for the status of an employee or consultant. May be consultant, hourly, on leave, active, retired etc. This list may be derived from the human resource department.','USER','dbo','TABLE','RA_EMPLOYEE_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ENCODING TYPE: Use this table to record all possible names, codes and other identifiers for the type of encoding that has been applied to a digital file. May include security encryption, zipping or other compression, RODE and so on.','USER','dbo','TABLE','RA_ENCODING_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ENHANCED RECOVERY TYPE: Use this table to record all possible names, codes and other identifiers for the types of method used for enhanced recovery of petroleum substances.','USER','dbo','TABLE','RA_ENHANCED_REC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ENTITLEMENT ACCESS TYPE: Use this table to record all possible names, codes and other identifiers for the type of access entitlement that is described in the row, such as read, write, delete for database access. For other types of access may be copy, view, sell etc.','USER','dbo','TABLE','RA_ENT_ACCESS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ENTITLEMENT COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of entitlement component, or the reason why a business object is associated with this entitlement. For example, a contract may be associated because it governs the conditions of the entitlement, or a seismic set may be associated because access to its acquisition products are controled by the entitlement.','USER','dbo','TABLE','RA_ENT_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ENTITLEMENT EXIRY ACTION: Use this table to record all possible names, codes and other identifiers for an action that must occur after the entitlement has expired. For example all copies of the relevant data must be destroyed.','USER','dbo','TABLE','RA_ENT_EXPIRY_ACTION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ENTITLEMENT SECURITY GROUP TYPE: Use this table to record all possible names, codes and other identifiers for the kind of security group that has been created, such as reference table updaters, land administrators, project teams, committeesetc.','USER','dbo','TABLE','RA_ENT_SEC_GROUP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ENTITLEMENT SECURITY GROUP CROSS REFERENCE: Use this table to record all possible names, codes and other identifiers for the type of relationship between groups, such as a group that governs another, or is part of another, turns into another, or replaces another or works in conjunction with (perhaps with a slightly different role).','USER','dbo','TABLE','RA_ENT_SEC_GROUP_XREF';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ENTITLEMENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of entitlement that is described in the row, such as a seismic lease data entitlement, a security based entitlement etc.','USER','dbo','TABLE','RA_ENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ENVIRONMENT TYPE: Use this table to record all possible names, codes and other identifiers for the environment in which operations occur or data is collected (marine, land, transition)','USER','dbo','TABLE','RA_ENVIRONMENT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS EQUIPMENT BUSINESS ASSOCIATE ROLE TYPE: Use this table to record all possible names, codes and other identifiers for the role of the business associate, such as rentor, owner, operator, authorized maintenance etc.','USER','dbo','TABLE','RA_EQUIP_BA_ROLE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS EQUIPMENT COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a piece of equipment.','USER','dbo','TABLE','RA_EQUIP_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TYPICAL EQUIPMENT INSTALLATION TYPE: Use this table to record all possible names, codes and other identifiers for the location where this type of equipment would normally be used, such as on the drilling assembly, in the well bore, on well site, on rig, in processing facility.','USER','dbo','TABLE','RA_EQUIP_INSTALL_LOC';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MAINTENANCE LOCATION TYPE: Use this table to record all possible names, codes and other identifiers for whether the maintenance activity was conducted on site, off site or in some specified location (Delaware warehouse) or type of location (such a maintenance yard).','USER','dbo','TABLE','RA_EQUIP_MAINT_LOC';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MAINTENANCE REASON: Use this table to record all possible names, codes and other identifiers for the reason why this maintenance activity was undertaken, such as preventative maintenance, predictive maintenance, failure etc.','USER','dbo','TABLE','RA_EQUIP_MAINT_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS EQUIPMENT MAINTENANCE STATUS: Use this table to record all possible names, codes and other identifiers for the status of a maintenance event for a piece of equipment, such as a pump. the status is described in a specific context (MAINT STATUS TYPE), such as financial, operational, or preventative.','USER','dbo','TABLE','RA_EQUIP_MAINT_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MAINTAIN STATUS TYPE: Use this table to record all possible names, codes and other identifiers for the type of status, or perspective, from which the status of a maintenance event is viewed, such as operational, financial etc.','USER','dbo','TABLE','RA_EQUIP_MAINT_STAT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS EQUIPMENT REMOVAL REASON: Use this table to record all possible names, codes and other identifiers for the reason why this particular piece of equipment was replaced, such as replace due to wear and tear (scheduled), replace due to failure, upgrade.','USER','dbo','TABLE','RA_EQUIP_REMOVE_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS EQUIPMENT SPECIFICATION: Use this table to record all possible names, codes and other identifiers for the specification or callibration type of measurement that is captured for a specific piece of equipment.','USER','dbo','TABLE','RA_EQUIP_SPEC';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS EQUIPMENT SPECIFICATION REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the kind of referece point used to describe specifications. For example, if the specifications being captured are tank strappings, the SPEC TYPE = TANK STRAPPING and SPEC REF TYPE = STRAPPING MARKERS and the REFERENCED VALUE = the height measure on the tank.','USER','dbo','TABLE','RA_EQUIP_SPEC_REF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS EQUIPMENT SPECIFICATION SET TYPE:  Use this table to record all possible names, codes and other identifiers for the kinds of specification sets that are created.','USER','dbo','TABLE','RA_EQUIP_SPEC_SET_TYPE';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_EQUIP_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS EQUIPMENT STATUS TYPE: Use this table to record all possible names, codes and other identifiers for valid types for classifying or grouping status information. Can include financial, operational condition etc.','USER','dbo','TABLE','RA_EQUIP_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS EQUIPMENT SYSTEM CONDITION: Use this table to record all possible names, codes and other identifiers for conditions that equipment must be in for maintenance to occur, such as shut down, moved to repair yard etc.','USER','dbo','TABLE','RA_EQUIP_SYSTEM_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS EQUIPMENT USE STATISTIC TYPE: Use this table to record all possible names, codes and other identifiers for use statistics which are widely varied in nature, depending on the type of equipment you are tracking. You may need to track distance driven, distance drilled, total revolutions, total cost of operations etc.','USER','dbo','TABLE','RA_EQUIP_USE_STAT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS EQUIPMENT CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of relationship between two pieces of equipment, often indicating one piece that can or has replaced another. May also be used to indicate equipment that has the same function, and are therefore equivalent. May be used to indicate the installation of one piece of equipment with or in another.','USER','dbo','TABLE','RA_EQUIP_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS EAST-WEST DIRECTION: Use this table to record all possible names, codes and other identifiers for valid East-West directions. For example, East, West.','USER','dbo','TABLE','RA_EW_DIRECTION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS EAST WEST START LINE: Use this table to record all possible names, codes and other identifiers for valid east-west starting lines for offset distances. This is used primarily for non-orthonormal survey blocks such as Texas surveys and California blocks. For example, FEL first east line, NEL northmost east line,...','USER','dbo','TABLE','RA_EW_START_LINE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FACILITY CLASSIFICATION TYPE: Use this table to record all possible names, codes and other identifiers for the type of classification assigned to the facility, such as sour gas. Often has a bearing on environmental restrictions and requirements.','USER','dbo','TABLE','RA_FACILITY_CLASS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FACILITY COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a facility','USER','dbo','TABLE','RA_FACILITY_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FACILITY SPECIFICATION CODE: Use this table to record all possible names, codes and other identifiers for a code for a specification where the result is a text string, rather than a number, and the text string should be validated against alist of values. General narrative descriptions can be stored in FACILITY_DESCRIPTION.SPEC_DESC.','USER','dbo','TABLE','RA_FACILITY_SPEC_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FACILITY SPECIFICATION TYPE: Use this table to record all possible names, codes and other identifiers for the specification measurement type that is captured for a specific facility. Try not to get confused with equipment specifications.','USER','dbo','TABLE','RA_FACILITY_SPEC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FACILITY STATUS: Use this table to record all possible names, codes and other identifiers for the status of the facility, such as ACTIVE, PENDING, DECOMMISSIONED etc. Defined in terms of a type of status','USER','dbo','TABLE','RA_FACILITY_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FACILITY TYPE: Use this table to record all possible names, codes and other identifiers for the codes classifying the facility according to its physical equipment or principal service performed.','USER','dbo','TABLE','RA_FACILITY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FACILITY CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of relationship between facilities, such as a component facility comprising part of a larger facility, a facility attached to another facility etc.','USER','dbo','TABLE','RA_FACILITY_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FACILITY FUNCTION: Use this table to record all possible names, codes and other identifiers for valid functions that are satisifed by a facility, such as measurement, transportation, processing, storage, seperation etc.','USER','dbo','TABLE','RA_FAC_FUNCTION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FACILITY LICENSE CONDITION TYPE: Use this table to record all possible names, codes and other identifiers for the type of condition applied to the facility license, such as flaring rate, venting rate, production rate, H2S content limit, emissionsetc.','USER','dbo','TABLE','RA_FAC_LIC_COND';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_FAC_LIC_COND_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DUE CONDITION: Use this table to record all possible names, codes and other identifiers for the state that must be achieved for the condition to become effective. For example, a report may be due 60 days after operations commence (or cease).','USER','dbo','TABLE','RA_FAC_LIC_DUE_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FACILITY LICENSE EXTENSION CONDITION: Use this table to record all possible names, codes and other identifiers for the criteria that must be addressed in order for the primary term of the license to be extended. For example, construction must be started etc.','USER','dbo','TABLE','RA_FAC_LIC_EXTEND_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS VIOLATION TYPE: Use this table to record all possible names, codes and other identifiers for the type of violation of a license that is being recorded. Can be as simple as failure to submit necessary reports or something more difficult such as improper procedures.','USER','dbo','TABLE','RA_FAC_LIC_VIOLATION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LICENSE VIOLATION RESOLUTION TYPE: Use this table to record all possible names, codes and other identifiers for the type of resolution to a violation of a license term, such as the payment of a fine or creation of new processes.','USER','dbo','TABLE','RA_FAC_LIC_VIOL_RESOL';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_FAC_MAINTAIN_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FACILITY MAINTAIN STATUS: Use this table to record all possible names, codes and other identifiers for the status of a facility maintenace event, such as approved, started, underway, completed, inspected etc. Note that statuses are defined within the framework of a point of view, such as operational, financial etc.','USER','dbo','TABLE','RA_FAC_MAINT_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FACILITY MAINTAIN STATUS TYPE: Use this table to record all possible names, codes and other identifiers for the type or perspective of status for a facility maintenance event, such as operational, financial, legal etc.','USER','dbo','TABLE','RA_FAC_MAINT_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PIPELINE COVER TYPE: Use this table to record all possible names, codes and other identifiers for valid types of material that covers or surrounds a pipeline that is buried below ground level (or sea level).','USER','dbo','TABLE','RA_FAC_PIPE_COVER';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PIPELINE MATERIAL: Use this table to record all possible names, codes and other identifiers for the material that a pipeline is constructed from, such as 24 pound steel etc.','USER','dbo','TABLE','RA_FAC_PIPE_MATERIAL';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PIPELINE TYPE: Use this table to record all possible names, codes and other identifiers for valid types of pipelines.','USER','dbo','TABLE','RA_FAC_PIPE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FACILITY SPECIFICATION REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of reference that a specification is measured against. For examples, a tank may store various volumes at specific pressures or temperatures.','USER','dbo','TABLE','RA_FAC_SPEC_REFERENCE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FACILITY STATUS TYPE: Use this table to record all possible names, codes and other identifiers for the types of status that may be tracked for a facility, such as construction, production, reclamation, operational, flaring etc.','USER','dbo','TABLE','RA_FAC_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FAULT TYPE: Use this table to record all possible names, codes and other identifiers for the type of fault. For example normal, reverse, strike, slip, or thrust.','USER','dbo','TABLE','RA_FAULT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FIELD COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a field.','USER','dbo','TABLE','RA_FIELD_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FIELD STATION TYPE: Use this table to record all possible names, codes and other identifiers for the type of field station, such as measured section, outcrop etc.','USER','dbo','TABLE','RA_FIELD_STATION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FIELD TYPE: Use this table to record all possible names, codes and other identifiers for the type of field. For example regulatory or locally assigned.','USER','dbo','TABLE','RA_FIELD_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE FINANCE ALIAS COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the reason why the component is associated with the AFE, such as drilling costs, processing costs, land bid costs etc.','USER','dbo','TABLE','RA_FIN_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FINANCE CENTER COST TYPE: Use this table to record all possible names, codes and other identifiers for the type of cost associated with the AFE or cost center.','USER','dbo','TABLE','RA_FIN_COST_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FINANCE STATUS: Use this table to record all possible names, codes and other identifiers for the current status of the financial reference, such as waiting for approval, closed out, active etc.','USER','dbo','TABLE','RA_FIN_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FINANCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of financial reference, such as AFE, cost center, legder etc.','USER','dbo','TABLE','RA_FIN_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FINANCE CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of relationship between cost center numbers or AFEs. Could be subordinate, replacement or a detail AFE for example.','USER','dbo','TABLE','RA_FIN_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FLUID TYPE: Use this table to record all possible names, codes and other identifiers for the type of fluids or substances produced by a well or used for various operations. For example oil, gas, mud or water. NOTE: This reference table is still being evaluated for possible subtyping.','USER','dbo','TABLE','RA_FLUID_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FONT: Use this table to record all possible names, codes and other identifiers for valid fonts, such as ARIAL or TIMES NEW ROMAN. Fonts are designs that govern the types of characters and symbols that can be displayed, and the design or apperance of those displays.','USER','dbo','TABLE','RA_FONT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FONT EFFECT: Use this table to record all possible names, codes and other identifiers for the special effect assigned to this display, such as bold, italic, normal.','USER','dbo','TABLE','RA_FONT_EFFECT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FOOTAGE ORIGIN: Use this table to record all possible names, codes and other identifiers for the valid points of origin used in measuring the survey footage calls to a well location.','USER','dbo','TABLE','RA_FOOTAGE_ORIGIN';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FOSSIL TAXON LEAF NAME ALIAS REASON or TYPE: Use this table to record all possible names, codes and other identifiers for the type of taxon leaf alias name that has been created.','USER','dbo','TABLE','RA_FOS_ALIAS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FOSSIL ASSEMBLAGE TYPE: Use this table to record all possible names, codes and other identifiers for a type of fossil assemblage, such as formal, zonal, working, informal etc.','USER','dbo','TABLE','RA_FOS_ASSEMBLAGE_TYPE';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_FOS_DESC_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FOSSIL DESCRIPTION TYPE: Use this table to record all possible names, codes and other identifiers for valid description types for fossils. May include descriptors such as color, size, spines, shape, composition etc.','USER','dbo','TABLE','RA_FOS_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FOSSIL LIFE HABIT: Use this table to record all possible names, codes and other identifiers for the life habit of the fossil, or where it typically is found during life, such as benthic, planctonic etc.','USER','dbo','TABLE','RA_FOS_LIFE_HABIT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FOSSIL NAME SET TYPE: Use this table to record all possible names, codes and other identifiers for the type of fossil name set, such as MMS, GSC, working or archival.','USER','dbo','TABLE','RA_FOS_NAME_SET_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FOSSIL OBSERVATION TYPE: Use this table to record all possible names, codes and other identifiers for the type of observation that is recorded, such as lithology, structure, fossil condition etc.','USER','dbo','TABLE','RA_FOS_OBS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FOSSIL TAXONOMIC GROUP: Use this table to record all possible names, codes and other identifiers for the taxonomic group that has been assigned to a fossil such as ostracod, diatom, foraminifera etc.','USER','dbo','TABLE','RA_FOS_TAXON_GROUP';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FOSSILTAXONOMIC LEVEL: Use this table to record all possible names, codes and other identifiers for the level of the taxonomic hierarchy at which this leaf has been identified, such as species, sub species, genus, sub genus etc.','USER','dbo','TABLE','RA_FOS_TAXON_LEVEL';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS FOSSIL TAXONOMIC GROUP: Use this table to record all possible names, codes and other identifiers for the taxonomic group that has been assigned to a fossil. Fossils may belong to a genus, subgenus, species or subspecies.','USER','dbo','TABLE','RA_FOS_XREF';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS GAS ANALYSIS VALUE CODE: Use this table to record all possible names, codes and other identifiers for the code assigned to the analysis property by observation, in cases where numeric values are not used.','USER','dbo','TABLE','RA_GAS_ANL_VALUE_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS GAS ANALYSIS VALUE TYPE: Use this table to record all possible names, codes and other identifiers for the type of text values for the gas chromatography.','USER','dbo','TABLE','RA_GAS_ANL_VALUE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS GRANTED RIGHT TYPE: Use this table to record all possible names, codes and other identifiers for the type of right granted to the holder. May include title, lease, P and NG lease, license, Permit P and NG, SDL, SDA, Exploration license, production license, drilling license, JOA, Pooling agreement etc. Called Document type by some systems.','USER','dbo','TABLE','RA_GRANTED_RIGHT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS HEAT CONTENT METHOD: Use this table to record all possible names, codes and other identifiers for the types of methods used to measure or calculated the heat content of a gas sample.','USER','dbo','TABLE','RA_HEAT_CONTENT_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS HOLE CONDITION: Use this table to record all possible names, codes and other identifiers for the condition of the wellbore. For example washed-out, sluffed or mud cake.','USER','dbo','TABLE','RA_HOLE_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS HORIZONTAL DRILLING REASON CODE: Use this table to record all possible names, codes and other identifiers for the reason for drilling a horizontal well. For example, some of the reasons for drilling a horizontal well are: Water coning, Intersecting a fracture system or Incr easing productivity.','USER','dbo','TABLE','RA_HORIZ_DRILL_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS HORIZONTAL DRILLING TYPE: Use this table to record all possible names, codes and other identifiers for the type of horizontal drilling. For example, Steered-bottom hole assembly or Non-steered.','USER','dbo','TABLE','RA_HORIZ_DRILL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INCIDENT COMPONENT ROLE: Use this table to record all possible names, codes and other identifiers for the role that an object plays in an HSE incident.','USER','dbo','TABLE','RA_HSE_COMP_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INCIDENT COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with the incident, such as well, building, facility etc. Use the foreign keys to create associations to the specific objects.','USER','dbo','TABLE','RA_HSE_INCIDENT_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INCIDENT DETAIL: Use this table to record all possible names, codes and other identifiers for the details about the incident, such as specific things that happened. Each thing that happened should be tracked at the level necessary for reporting and analysis.','USER','dbo','TABLE','RA_HSE_INCIDENT_DETAIL';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INCIDENT ACTION RESPONSE TYPE: Use this table to record all possible names, codes and other identifiers for valid types of action taken in response to an incident, such as evacuation, called air ambulance, shut down, apply first aid etc.','USER','dbo','TABLE','RA_HSE_RESPONSE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS IMAGE CALIBRATION METHOD: Use this table to record all possible names, codes and other identifiers for the method used to calibrate an image, such as manual, interpolation, scale detection etc.','USER','dbo','TABLE','RA_IMAGE_CALIBRATE_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS IMAGE SECTION TYPE: Use this table to record all possible names, codes and other identifiers for the type of section on an image, such as header, tool configuration, well diagram, upper scale, lower scale, section, repeat pass, high resolution etc.','USER','dbo','TABLE','RA_IMAGE_SECTION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INCIDENT BA ROLE: Use this table to record all possible names, codes and other identifiers for the role or function of a party in an incident, such as victim, medic, safety officer etc.','USER','dbo','TABLE','RA_INCIDENT_BA_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INCIDENT CAUSE CODE: Use this table to record all possible names, codes and other identifiers for a code that refines the general cause of an incident.','USER','dbo','TABLE','RA_INCIDENT_CAUSE_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INCIDENT CAUSE TYPE: Use this table to record all possible names, codes and other identifiers for valid causes of an event, such as negligence, equipment failure, act of God, Act of Terrorism, vandalism or human error.','USER','dbo','TABLE','RA_INCIDENT_CAUSE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS HSE INCIDENT INTERACTION TYPE: Use this table to record all possible names, codes and other identifiers for the type of interaction among components of an incident.','USER','dbo','TABLE','RA_INCIDENT_INTERACT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INCIDENT RESPONSE RESULT: Use this table to record all possible names, codes and other identifiers for the result of the action taken, where applicable. May be used to indicate what actions are successful and have the desired result.','USER','dbo','TABLE','RA_INCIDENT_RESP_RESULT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS HSE INCIDENT SUBSTANCE: Use this table to record all possible names, codes and other identifiers for any substance involved with an HSE incident. This may be a hydrocarbon, a drilling fluid, fire retardent etc.','USER','dbo','TABLE','RA_INCIDENT_SUBSTANCE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS HSE INCIDENT SUBSTANCE ROLE: Use this table to record all possible names, codes and other identifiers for the role played by a substance in an HSE Incident. Could be a spilled substance, used as fire retardant or used in first aid.','USER','dbo','TABLE','RA_INCIDENT_SUBST_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INFORMATION PROCESS: Use this table to record all possible names, codes and other identifiers for the technical transformation process used to generate one technical item from another. For seismic trace data, this may be the app lication of flattening or migration algorithms. For survey data, this may be the computation of raw survey notes.','USER','dbo','TABLE','RA_INFORMATION_PROCESS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INPUT TYPE: Use this table to record all possible names, codes and other identifiers for the type of input into an electrical device. Usually measured in Watts.','USER','dbo','TABLE','RA_INPUT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INSPECTION COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component that is associated. Can be a broker (if it is a Business Associate) or the inspected document (if it is an information item) or an inspected line ( if it is a seismic set).','USER','dbo','TABLE','RA_INSP_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INSPECTION STATUS: Use this table to record all possible names, codes and other identifiers for the status of this inspection, such as completed, waiting on approval, waiting for client decision etc.','USER','dbo','TABLE','RA_INSP_STATUS';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_INSTRUMENT_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND INSTRUMENT TYPE: Use this table to record all possible names, codes and other identifiers for the Land Instrument type. May be caveat, Cert of non dev, assignment, mortgage, discharge etc','USER','dbo','TABLE','RA_INSTRUMENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INSTRUMENT DETAIL CODE: In the case that the instrument detail is described as a coded value, use this table to record all possible names, codes and other identifiers for valid codes for each type of detail.','USER','dbo','TABLE','RA_INST_DETAIL_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INSTRUMENT DETAIL REFERENCE VALUE TYPE: In the case where a detail is referenced to some other value (such as a submission due after a certain period, or a date or an activity), use this table to record all possible names, codes and other identifiers for the type of reference value captured here. The values, if relevant, are stored in associated columns.','USER','dbo','TABLE','RA_INST_DETAIL_REF_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INSTRUMENT DETAIL TYPE: Use this table to record all possible names, codes and other identifiers for the kind of detail information about the instrument that has been stored.','USER','dbo','TABLE','RA_INST_DETAIL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ORIGIN TYPE: Use this table to record all possible names, codes and other identifiers for the type of originating source of the interpretation. This could be a tape or disk stored in the Records module or an intermediate or final output product from a processing flow. The latter are best used in interpretation systems where the interpreted product may be ephemeral or stored only within the processing system.','USER','dbo','TABLE','RA_INTERP_ORIGIN_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INTERPRETATION TYPE: Use this table to record all possible names, codes and other identifiers for the type of interpretation, such as time, depth, amplitude, shifts etc.','USER','dbo','TABLE','RA_INTERP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INTEREST SET COMPONENT: Use this table to record all possible names, codes and other identifiers for the type of component that belong to the interest set, such as wells, land rights or wrongs, contracts and obligations.','USER','dbo','TABLE','RA_INT_SET_COMPONENT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BA INTEREST SET ROLE: Use this table to record all possible names, codes and other identifiers for the role played by a partner in the interest set, such as operator.','USER','dbo','TABLE','RA_INT_SET_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INTEREST SET STATUS: Use this table to record all possible names, codes and other identifiers for the status of a partnership, from a planning and approval perspective or an operational perspective. The status of the partnership from various perspectives (legal, finance, operations, land managers etc) may be tracked.','USER','dbo','TABLE','RA_INT_SET_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INTEREST SET STATUS TYPE: Use this table to record all possible names, codes and other identifiers for the perspective from which the status is measured, such as financial, operational, legal, regulatory etc.','USER','dbo','TABLE','RA_INT_SET_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BA INTEREST SET TRIGGER: Use this table to record all possible names, codes and other identifiers for the event that triggered a change in the interest set shares or roles. When the event occurs, a new row in INTEREST SET is created using a new SEQUENCE NUMBER to identify the new version of the interest set.','USER','dbo','TABLE','RA_INT_SET_TRIGGER';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BA INTEREST SET TYPE: Use this table to record all possible names, codes and other identifiers for the type of interest set, such as working, royalty etc.','USER','dbo','TABLE','RA_INT_SET_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BA INTEREST SET CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of relationship between interest sets. Interest sets may supercede each other, have an impact on the net worth of the interest set etc.','USER','dbo','TABLE','RA_INT_SET_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INVENTORY MATERIAL TYPE: Use this table to record all possible names, codes and other identifiers for the type of material that is tracked in a general sense. Specific kinds of equipment should be tracked in CAT EQUIPMENT and specific kindsof additives should be tracked in CAT ADDITIVE.','USER','dbo','TABLE','RA_INV_MATERIAL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS INFORMATION ITEM CATEGORY: Use this table to record all possible names, codes and other identifiers for the category of information item, such as May be Acquisition support products, trace products etc.','USER','dbo','TABLE','RA_ITEM_CATEGORY';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_ITEM_SUB_CATEGORY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND ACQUISITION MEHOD: Use this table to record all possible names, codes and other identifiers for the method used to acquire the rights to this land right. May be purchase, lease, license, partnership, farmin, farmout, rental etc.','USER','dbo','TABLE','RA_LAND_ACQTN_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND AGREEMENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of land agreement, can be an additional qualification of LAND RIGHT.GRANTED RIGHT TYPE, for more descriptive details about the type of granted right.','USER','dbo','TABLE','RA_LAND_AGREE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND BIDDER TYPE: Use this table to record all possible names, codes and other identifiers for the land bidder type such as broker, self, or partner.','USER','dbo','TABLE','RA_LAND_BIDDER_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND BID STATUS: Use this table to record all possible names, codes and other identifiers for Land Bid Status such as: pending, successful, unsucessful etc.','USER','dbo','TABLE','RA_LAND_BID_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND BID TYPE: Use this table to record all possible names, codes and other identifiers for the Land bid type. May be sliding scale, grouped, straight.','USER','dbo','TABLE','RA_LAND_BID_TYPE';
go

execute sp_addextendedproperty 'Description','R LAND CASE ACTION: Use this table to record all possible names, codes and other identifiers for the last action made to the case file.','USER','dbo','TABLE','RA_LAND_CASE_ACTION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND CASE TYPE: Use this table to record all possible names, codes and other identifiers for the Land Case Type such as: timber, geothermal....','USER','dbo','TABLE','RA_LAND_CASE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CASH BID TYPE: Use this table to record all possible names, codes and other identifiers for the type of cash bid. This is used to determine the method used to process the complete bid. May be a sliding scale bid, group bid... In a sliding scale bid, bids are placed on parcels in order of importance - if the first priority bid is accepted, the second bid may or may not be considered (depending on whether the bid is contingent on acceptance). In a grouped bid, all parcels with the same priority must be accepted or rejected together. Not to be used for Work bids.','USER','dbo','TABLE','RA_LAND_CASH_BID_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component that is associated with the land right, such as a contract or a facility.','USER','dbo','TABLE','RA_LAND_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND LESSOR TYPE: Use this table to record all possible names, codes and other identifiers for the type of lessor, such as federal, indian, publ ic, BIA, Aboriginal','USER','dbo','TABLE','RA_LAND_LESSOR_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND OFFERING STATUS: Use this table to record all possible names, codes and other identifiers for the Land Offering Status such as: postponed, cancelled, withdrawn, active, sold, not sold.','USER','dbo','TABLE','RA_LAND_OFFRING_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND OFFERING TYPE: Use this table to record all possible names, codes and other identifiers for Land Offering types such as: state, indian, federal, BLM, first nations, provincial, OCS, crown.','USER','dbo','TABLE','RA_LAND_OFFRING_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND PROPERTY TYPE: Use this table to record all possible names, codes and other identifiers for the property designation for reporting acreages, such as core, non core, core developed, core non developed e tc.','USER','dbo','TABLE','RA_LAND_PROPERTY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND REF NUM TYPE: Use this table to record all possible names, codes and other identifiers for the type of reference number, such as previous title number, government number etc.','USER','dbo','TABLE','RA_LAND_REF_NUM_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND RENTAL TYPE: Use this table to record all possible names, codes and other identifiers for Land rental types. A delay rental is made to defer requirement to drill during the primary term of a lease. An annual rental is made in addition to any subsequent royalty payment due to production. Shut in royalty is made in lieu of the royalty payment and is usually equivalent to the delay or rental amount, or can be on a well by well basis.','USER','dbo','TABLE','RA_LAND_RENTAL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND REQUEST TYPE: Use this table to record all possible names, codes and other identifiers for the type of request that was made, such as a Call for Nominations or a Posting Request. Typically, a Call for Nominations is created by a regulatory agency (in Canada, this is done by Yukon, Nortwest Territory and Nunuvit). Industry responds with posting requests, usually the company that creates a posting request is obligated to bid on the resultant land sale offering.','USER','dbo','TABLE','RA_LAND_REQUEST_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND REQUEST STATUS: Use this table to record all possible names, codes and other identifiers for a Land Request staus such as: pending, refused, accepted.','USER','dbo','TABLE','RA_LAND_REQ_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND RIGHT CATEGORY: Use this table to record all possible names, codes and other identifiers for the category of land right. May be Mineral or Surface.','USER','dbo','TABLE','RA_LAND_RIGHT_CATEGORY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND RIGHT STATUS: Use this table to record all possible names, codes and other identifiers for Land Right Statuses such as: continued, held by production, termination, inactivated, surrendered.','USER','dbo','TABLE','RA_LAND_RIGHT_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND STATUS TYPE: Use this table to record all possible names, codes and other identifiers for the type of status for a land right, such as legal, financial or working.','USER','dbo','TABLE','RA_LAND_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND TITLE CHANGE REASON: Use this table to record all possible names, codes and other identifiers for Land Title Change Reasons such as: seperation, consolodation, transfer of land or interest.','USER','dbo','TABLE','RA_LAND_TITLE_CHG_RSN';
go

execute sp_addextendedproperty 'Description','REFERENCE LAND TITLE TYPE: Use this table to record all possible names, codes and other identifiers for the type of land title held.','USER','dbo','TABLE','RA_LAND_TITLE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND UNIT TRACT TYPE: Use this table to record all possible names, codes and other identifiers for the type of land unit tract.','USER','dbo','TABLE','RA_LAND_TRACT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND UNIT TYPE: Use this table to record all possible names, codes and other identifiers for the type of land unit.','USER','dbo','TABLE','RA_LAND_UNIT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND WELL RELATIONSHIP TYPE: Use this table to record all possible names, codes and other identifiers for the type of relationship between the well and the land right. For example, a well may be inferred to be related to a land right because of its location, or the relationship may be explicitly stated in an agreement. In some cases, a well may not be located physically in a land right in order to have an association.','USER','dbo','TABLE','RA_LAND_WELL_REL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LANGUAGE: Use this table to record all possible names, codes and other identifiers for the form of a means of communicating ideas or feelings by the use of conventionalized signs, sounds, gestures, or marks having understood meanings. Usually the language used in a document, proceding, process or appllication.','USER','dbo','TABLE','RA_LANGUAGE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LEASE UNIT STATUS: Use this table to record all possible names, codes and other identifiers for the operational or legal status of the production lease or unit.','USER','dbo','TABLE','RA_LEASE_UNIT_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE LEASE UNIT TYPE: Use this table to record all possible names, codes and other identifiers for the type of production lease or unit. For example, Federal Lease, State Lease , Indian Lease, Production Unit, etc.','USER','dbo','TABLE','RA_LEASE_UNIT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LEGAL SURVEY TYPE: Use this table to record all possible names, codes and other identifiers for valid survey types used for legal descriptions. For example, Carter, Congressional, Dominion Land Survey, ...','USER','dbo','TABLE','RA_LEGAL_SURVEY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LICENSE STATUS: Use this table to record all possible names, codes and other identifiers for the status of the license, such as pending, approved, terminated, cancelled by operator, denied, extended etc.','USER','dbo','TABLE','RA_LICENSE_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LICENSE STATUS TYPE: Use this table to record all possible names, codes and other identifiers for the type of status described for the license. Could be working, file, activity, regulatory, environmental etc. Used to track the situation where multiple types of statuses are to be tracked.','USER','dbo','TABLE','RA_LIC_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LINER TYPE: Use this table to record all possible names, codes and other identifiers for the type of liner used in the borehole. For example, slotted, gravel packed or pre-perforated etc.','USER','dbo','TABLE','RA_LINER_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LITHOLOGY: Use this table to record all possible names, codes and other identifiers for the major lithologic types. For example sandstone, limestone, dolomite or shale.','USER','dbo','TABLE','RA_LITHOLOGY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LITHOLOGIC ABUNDANCE: Use this table to record all possible names, codes and other identifiers for the relative abundance of each color rank (first, second, third:1, 2, 3) or as a ranked magnitude (abundant, common, rare). Used in the litholgy model.','USER','dbo','TABLE','RA_LITH_ABUNDANCE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LITHOLOGIC BOUNDARY TYPE: Use this table to record all possible names, codes and other identifiers for the type of boundary occurring between two adjacent rock intervals (e.g., unconformable, nonconformable, conformable, etc.).','USER','dbo','TABLE','RA_LITH_BOUNDARY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LITHOLOGIC COLOR: Use this table to record all possible names, codes and other identifiers for the basic color or color adjective of lithologic components such as red, grey, blue etc. Used in Lithology','USER','dbo','TABLE','RA_LITH_COLOR';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LITHOLOGIC CONSOLIDATION: Use this table to record all possible names, codes and other identifiers for consolidation or induration of the rock sample. Induration of a rock sample (sandstone) refers to its resistance to physical disaggregation and does not necessarily refer to hardness of the constituent grains. Common types of consolidation (induration) are, dense, hard, medium hard, soft, spongy or friable.','USER','dbo','TABLE','RA_LITH_CONSOLIDATION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CYCLE BED: Use this table to record all possible names, codes and other identifiers for a sequence of beds or related processes and conditions, repeated in the same order that is recorded in a sedimentary deposit.','USER','dbo','TABLE','RA_LITH_CYCLE_BED';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LITHOLOGIC DEPOSITIONAL ENVIRONMENT: Use this table to record all possible names, codes and other identifiers for the type of interpreted environment of the deposition. A depositional environment is the physical environment in which sediments were deposited. For example, a high-energy river channel or a carbonate barrier reef.','USER','dbo','TABLE','RA_LITH_DEP_ENV';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LITHOLOGIC DIAGENESIS TYPE: Use this table to record all possible names, codes and other identifiers for the type of diagenesis or diagenetic mineral which is found in the described interval. Common types of diagenesis are compaction, cementation, recrystallization or dolomitization. Diagenetic minerals may include dolomite, glauconite, olivine, etc.','USER','dbo','TABLE','RA_LITH_DIAGENESIS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LITHOLOGIC DISTRIBUTION: Use this table to record all possible names, codes and other identifiers that describe the distribution of the rock color in the sub-interval. For example, the color distribution maybe graded, uneven, etc.','USER','dbo','TABLE','RA_LITH_DISTRIBUTION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LITHOLOGIC INTENSITY: Use this table to record all possible names, codes and other identifiers for the rock color intensity. The color intensity is used to describe the amount of color associated with the sample. For example, the color intensity may range from high to low.','USER','dbo','TABLE','RA_LITH_INTENSITY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LITHOLOGY LOG COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a lithological log','USER','dbo','TABLE','RA_LITH_LOG_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LITHOLOGIC LOG TYPE: Use this table to record all possible names, codes and other identifiers for the type of log. May be interpretive, percentage, qualified percentage, composite interpretive or descriptive.','USER','dbo','TABLE','RA_LITH_LOG_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS OIL STAIN: Use this table to record all possible names, codes and other identifiers for the type of oil stain observed in the rock sample. For example, the oil stain can be described as fair live oil, questionable, etc','USER','dbo','TABLE','RA_LITH_OIL_STAIN';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGIC PATTERN CODE: Use this table to record all possible names, codes and other identifiers for Lithologic Pattern codes.','USER','dbo','TABLE','RA_LITH_PATTERN_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LITHOLOGIC ROCKPART: Use this table to record all possible names, codes and other identifiers for lithologic rockpart components such as glauconite (rock) or crinoids (fossil).','USER','dbo','TABLE','RA_LITH_ROCKPART';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LITHOLOGIC ROCK MATRIX: Use this table to record all possible names, codes and other identifiers for the type of fine grain material filling voids between larger grains. The grained particles in a poorly sorted sedimentary rock. Matrix can be fine grained sandstone, siltstone, shale, etc.','USER','dbo','TABLE','RA_LITH_ROCK_MATRIX';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ROCK PROFILE: Use this table to record all possible names, codes and other identifiers for the type of rock weathering or borehole profile associated with the particular described interval. Examples of the rock profiles can be recessive, cliff, etc.','USER','dbo','TABLE','RA_LITH_ROCK_PROFILE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LITHOLOGIC ROCK TYPE: Use this table to record all possible names, codes and other identifiers for the type of rock comprising a significant portion of the interval. For example, the predominant rock type in the interval may be coal, or oolitic limestone or calcareous sandstone.','USER','dbo','TABLE','RA_LITH_ROCK_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LITHOLOGIC ROUNDING: Use this table to record all possible names, codes and other identifiers for the shape of the rock components. This is typically used in describing clastic sedimentary rocks. There are an almost infinite number of variations in the shapes of grain size, but the most common classes are sharp, angular, subangular, rounded or globular.','USER','dbo','TABLE','RA_LITH_ROUNDING';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LITHOLOGIC SCALE SCHEME: Use this table to record all possible names, codes and other identifiers for the type of scaling system source used to determine the grain size like the widely accepted Wentworth scale, or a company internal grade scale (Canstrat, Shell).','USER','dbo','TABLE','RA_LITH_SCALE_SCHEME';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LITHOLOGIC SORTING: Use this table to record all possible names, codes and other identifiers for that identify the type of sorting associated with the principal rock being described. This feature is most important in coarse clastic rocks andcommon examples are well, medium or poorly sorted.','USER','dbo','TABLE','RA_LITH_SORTING';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LITHOLOGIC STRUCTURE: Use this table to record all possible names, codes and other identifiers for the type of sedimentary or other rock structure occurring in the lithologic interval being described (e.g., cross-stratification, mud cracks, ripple marks, etc.). The rock structure may be non-sedimentary, such as contorted bedding or fault zone.','USER','dbo','TABLE','RA_LITH_STRUCTURE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LOCATION DESCRIPTION TYPE: Use this table to record all possible names, codes and other identifiers for valid types of location descriptions.','USER','dbo','TABLE','RA_LOCATION_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LOCATION QUALIFIER: Use this table to record all possible names, codes and other identifiers for valid types of locations. For example, Actual, Theorectical, Original, Contract, ... (but not Rationalized).','USER','dbo','TABLE','RA_LOCATION_QUALIFIER';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LOCATION QUALITY: Use this table to record all possible names, codes and other identifiers for the quality or degree of reliability of a location.','USER','dbo','TABLE','RA_LOCATION_QUALITY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LOCATION TYPE: Use this table to record all possible names, codes and other identifiers for the type of object being given a legal location. For example, node, well, ...','USER','dbo','TABLE','RA_LOCATION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LOG ARRAY DIMENSION: Use this table to record all possible names, codes and other identifiers for the dimension of the element in the well log parameter array.','USER','dbo','TABLE','RA_LOG_ARRAY_DIMENSION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LOG DIGIT CORRECTION METHOD: Use this table to record all possible names, codes and other identifiers for the method used to correct the log depth.','USER','dbo','TABLE','RA_LOG_CORRECT_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LOG CURVE CLASSIFICATION SYSTEM: Use this table to record all possible names, codes and other identifiers for the system used to generate this well log curve classification. Several systems are typically used. Value to the Customer system was created by SLB and is used by several logging companies. Other systems for classifications have been defined by logging contractors, the PWLS or E and P companies.','USER','dbo','TABLE','RA_LOG_CRV_CLASS_SYSTEM';
go

execute sp_addextendedproperty 'Description','LOG DEPTH TYPE: Use this table to record all possible names, codes and other identifiers for the type of depth measurements provided in the log, such as Measured (MD) or True Vertical (TVD)','USER','dbo','TABLE','RA_LOG_DEPTH_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LOG DIRECTION: Use this table to record all possible names, codes and other identifiers for the direction that the tool string was moving when the logging occured, usually UP or DOWN.','USER','dbo','TABLE','RA_LOG_DIRECTION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LOG GOOD VALUE TYPE: Use this table to record all possible names, codes and other identifiers for valid types of good values that are used to indicate the top and base of useful data gathered during logging operations.','USER','dbo','TABLE','RA_LOG_GOOD_VALUE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LOG INDEX TYPE: Use this table to record all possible names, codes and other identifiers for the type of measurement index for the log, such as depth or time.','USER','dbo','TABLE','RA_LOG_INDEX_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LOG MATRIX LITHOLOGY SETTING: Use this table to record all possible names, codes and other identifiers for the type of lithologic material present in the rock being evaluated. For example, sandstone, limestone.','USER','dbo','TABLE','RA_LOG_MATRIX';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LOG IMAGE POSITION TYPE: Use this table to record all possible names, codes and other identifiers for the type of position that is on the log section, such as top header, bottom header, left depth track, depth calibration, skew correction.','USER','dbo','TABLE','RA_LOG_POSITION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TOOL TYPE: Use this table to record all possible names, codes and other identifiers for the logging tools which compose a log tool string. For example, the type of wireline tool may be a compensated neutron tool, sonic tool etc.','USER','dbo','TABLE','RA_LOG_TOOL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LOST MATERIAL TYPE: Use this table to record all possible names, codes and other identifiers for the type of material used in treating a lost circulation interval. For example cane, chicken feather, hay or walnut hulls.','USER','dbo','TABLE','RA_LOST_MATERIAL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND RIGHT FACILITY CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of relationship between the land right and the facility, such as produciton, operated by etc.','USER','dbo','TABLE','RA_LR_FACILITY_XREF';
go

execute sp_addextendedproperty 'Description','LAND RIGHT FIELDCROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of relationship between the land right and the field, such as produciton, operated by etc.','USER','dbo','TABLE','RA_LR_FIELD_XREF';
go

execute sp_addextendedproperty 'Description','REFERENCE LAND RIGHT SIZE TYPE: Use this table to record all possible names, codes and other identifiers for the type of size referenced, usually based on interest type, or to distinguish between onshore and offshore holdings.','USER','dbo','TABLE','RA_LR_SIZE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND RIGHTTERMINATION REQUIREMENTS: Use this table to record all possible names, codes and other identifiers for a valid list of requirements for the termination of agreenents','USER','dbo','TABLE','RA_LR_TERMIN_REQMT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND RIGHT TERMINATION TYPE: Use this table to record all possible names, codes and other identifiers for the Land Right Termination Type. This may be expiry, surrendor, trade, sale cancellation.','USER','dbo','TABLE','RA_LR_TERMIN_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND RIGHT XREF TYPE: Use this table to record all possible names, codes and other identifiers for the Land Right Cross Reference Type. This may be history , overlap, chain of title, mineral to c of T, lease to license etc','USER','dbo','TABLE','RA_LR_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND OFFER CANCEL REASON: Use this table to record all possible names, codes and other identifiers for the reason why the land sale offering was removed from the land sale, such as withdrawn, no bids, no acceptable bids.','USER','dbo','TABLE','RA_L_OFFR_CANCEL_RSN';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MACERAL AMOUNT TYPE: Use this table to record all possible names, codes and other identifiers for the description of the amount of maceral (trace, abundant). This is often a name that relates to a range of values, such as rare = <.1%. Thisis always going to be liptinite. Do not use ORGANIC MATTER TYPE (function is unclear).If used in petrology table, the meaning would need to be organic matter in coal or organic matter that is dispersedthrough the rocks or both. (Coal vs DOM dispersed organic matter - vs both). Check the Petrologytable to be sure we do properly.','USER','dbo','TABLE','RA_MACERAL_AMOUNT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MAINTENANCE PROCESS: Use this table to record all possible names, codes and other identifiers for the maintenance process used, such as tape rewind and tightening.','USER','dbo','TABLE','RA_MAINT_PROCESS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MATURATION TYPE: Use this table to record all possible names, codes and other identifiers for the level of maturity of a source rock or extracted organic material. May be immature, mature or over mature','USER','dbo','TABLE','RA_MATURATION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MATURITY METHOD: Use this table to record all possible names, codes and other identifiers for the type of method of maturation.','USER','dbo','TABLE','RA_MATURITY_METHOD';
go

execute sp_addextendedproperty 'Description','COMPRESSIBILITY FACTORE METHOD: Use this table to record all possible names, codes and other identifiers for the method used to determine the compresibility factor.','USER','dbo','TABLE','RA_MBAL_COMPRESS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CURVE FIT TYPE: Use this table to record all possible names, codes and other identifiers for the type of curve, such as manual fit, best fit, weighted best fit.','USER','dbo','TABLE','RA_MBAL_CURVE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MEASUREMENT LOCATION TYPE: Use this table to record all possible names, codes and other identifiers for the type of location the measurment was taken from. For example, center of the core, sidewall, etc.','USER','dbo','TABLE','RA_MEASUREMENT_LOC';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MEASUREMENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of measurement recorded.','USER','dbo','TABLE','RA_MEASUREMENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE MEASURE TECHNIQUE: Use this table to record all possible names, codes and other identifiers for the various flow measurement techniques used for well tests. For example orifice meter, separator or estimated.','USER','dbo','TABLE','RA_MEASURE_TECHNIQUE';
go

execute sp_addextendedproperty 'Description','MEDIA TYPE: Use this table to record all possible names, codes and other identifiers for the type of media used for the physical rendering of an item. Allowable types include 8 mm tape, 9 inch tape, backup unet, cassette, diskette, epoch, film, linen, microfilm, mylar, negative, original, paper, photo print, print, reproducable, sepia, xerox, worm optical disk, unknown, mixed.','USER','dbo','TABLE','RA_MEDIA_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS METHOD TYPE: Use this table to record all possible names, codes and other identifiers for the type of method being used in the sample analysis.','USER','dbo','TABLE','RA_METHOD_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MISCELLANEOUS DATA CODE: Use this table to record all possible names, codes and other identifiers for a value associated with a miscellaneous data type, in the case where the value is selected from a list of valid values.','USER','dbo','TABLE','RA_MISC_DATA_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MISCELLANEOUS DATA TYPE: Use this table to record all possible names, codes and other identifiers for the type of miscellaneous or generic data associated with an entity and stored at a general level. Examples: culture, safety, financial, regulatory, environment.','USER','dbo','TABLE','RA_MISC_DATA_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MISSING STRATIGRAPHY TYPE:  The alias table for missing stratigraphic section types.','USER','dbo','TABLE','RA_MISSING_STRAT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MOBILITY TYPE: Use this table to record all possible names, codes and other identifiers for the type of color the mobility marker used in the fluorescence analysis is. The mobility marker can be used to track any movement of the cells it is attached to or the marker can be used to visualize stable dissolution of different substances.','USER','dbo','TABLE','RA_MOBILITY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MONTH: Use this table to record all possible names, codes and other identifiers for a valid month. For example January, February etc.','USER','dbo','TABLE','RA_MONTH';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MUD COLLECTION REASON: Use this table to record all possible names, codes and other identifiers for the reason or business process behind the collection of the mud sample, such as logging or drilling.','USER','dbo','TABLE','RA_MUD_COLLECT_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MUD LOG COLOR: Use this table to record all possible names, codes and other identifiers for observed colors resulting from llithologic analysis, such as fluorescence_color (Color of hydrocarbon as viewed in ultraviolet light.) cut_color (Color of hydrocarbon extracted by reagent/solvent in ultraviolet light.','USER','dbo','TABLE','RA_MUD_LOG_COLOR';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MUD PROPERTY CODE: Use this table to record all possible names, codes and other identifiers for a validated code for the property being measured, in the case where the response is selected from a list of allowed values. In each case, the codes allowed are specific to the property type being measured.','USER','dbo','TABLE','RA_MUD_PROPERTY_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MUD PROPERTY REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of mud property reference that qualifies a mud property measurement, such as temperature.','USER','dbo','TABLE','RA_MUD_PROPERTY_REF';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MUD PROPERTY TYPE: Use this table to record all possible names, codes and other identifiers for the type of mud property being measured, such as alkalinity. Various properties may be measured using numbers, codes or textual descriptions.','USER','dbo','TABLE','RA_MUD_PROPERTY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE MUD SAMPLE TYPE: Use this table to record all possible names, codes and other identifiers for the type of mud sample used for determining mud resistivity. For example mud cake, filtrate or mud.','USER','dbo','TABLE','RA_MUD_SAMPLE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MUNICIPALITY: Use this table to record all possible names, codes and other identifiers for the name of the municipality.','USER','dbo','TABLE','RA_MUNICIPALITY';
go

execute sp_addextendedproperty 'Description','REFERENCE STRAT NAME SET CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of cross reference that exists between name sets. May be used to indicate that one name set has replaced another, that a name set is a subset of another, that strat unit names in a name set are automatically converted to another for data loads etc.','USER','dbo','TABLE','RA_NAME_SET_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS NODE POSITION: Use this table to record all possible names, codes and other identifiers for valid positions on a wellbore path. For example, surface point, bottomhole point, or kick-off point.','USER','dbo','TABLE','RA_NODE_POSITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS NORTH: Use this table to record all possible names, codes and other identifiers for valid north references used in surveying to define angular measurements. For example, True North, Magnetic North, Grid North, Astronomical North, ...','USER','dbo','TABLE','RA_NORTH';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS NOTIFICATION COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a notification.','USER','dbo','TABLE','RA_NOTIFICATION_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS NOTIFICATION TYPE:  the type of notification.','USER','dbo','TABLE','RA_NOTIFICATION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS NORTH-SOUTH DIRECTION: Use this table to record all possible names, codes and other identifiers for valid north-south Directions. For example, North, South.','USER','dbo','TABLE','RA_NS_DIRECTION';
go

execute sp_addextendedproperty 'Description','REFERENCE NORTH-SOUTH START LINE: Use this table to record all possible names, codes and other identifiers for valid north-south starting lines for offset distances. This is used primarily for non-orthonormal survey blocks, such as Texas surveys and Californiablocks. For example, FS L first south line, ESL eastmost south line, ...','USER','dbo','TABLE','RA_NS_START_LINE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS OBLIGATION CALCULATION METHOD: Use this table to record all possible names, codes and other identifiers for the method used to calculate the obligation.','USER','dbo','TABLE','RA_OBLIG_CALC_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS OBLIGATION CALCULATION POINT OF DEDUCTION: Use this table to record all possible names, codes and other identifiers for the point at which the calculation is taken, such as at the wellhead. May be for deduction or obligation calculation.','USER','dbo','TABLE','RA_OBLIG_CALC_POINT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS OBLIG CATEGORY: Use this table to record all possible names, codes and other identifiers for the Obligation Category, this may be one of non-payment obligation, rental, lease or royalty','USER','dbo','TABLE','RA_OBLIG_CATEGORY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS OBLIGATION COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with an obligation.','USER','dbo','TABLE','RA_OBLIG_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS OBLIGATION COMPONENT REASON: Use this table to record all possible names, codes and other identifiers for the reason why the component is associated with the obligation. For example, seismic data may be acquired to satisfy the terms of an obligation, a contract may govern the management of an obligation or a document may provide a record of the obligation.','USER','dbo','TABLE','RA_OBLIG_COMP_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS OBLIGATION DEDUCTION CALCULATION METHOD: Use this table to record all possible names, codes and other identifiers for the method used to calculate the deduction, such as the MIN/MAX method.','USER','dbo','TABLE','RA_OBLIG_DEDUCT_CALC';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS OBLIGATION PARTY TYPE: Use this table to record all possible names, codes and other identifiers for the type of party for the obligation, specifically whether he is a PAYEE or a PAYOR. Used to support identification of Burden Bearer relationships for obligations.','USER','dbo','TABLE','RA_OBLIG_PARTY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS OBLIGATION PAYMENT RESPONSIBILITY: Use this table to record all possible names, codes and other identifiers for whether the obligation is paid out entirely by one partner, each partner is responsible for paying only their share, or whether a third party has been retained to make payments and charge back to each partner.','USER','dbo','TABLE','RA_OBLIG_PAY_RESP';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS OBLIGATION REMARK TYPE: Use this table to record all possible names, codes and other identifiers for the type of remark, such as work obligation fulfillment, general, payment etc.','USER','dbo','TABLE','RA_OBLIG_REMARK_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS OBLIGATION SUSPEND PAYMENT REASON: Use this table to record all possible names, codes and other identifiers for the reason the payment was suspended, such as bankruptcy.','USER','dbo','TABLE','RA_OBLIG_SUSPEND_PAY';
go

execute sp_addextendedproperty 'Description','REFERENCE OBLIGATION TRIGGER METHOD: Use this table to record all possible names, codes and other identifiers for the means by which the obligation is triggered, and becomes active.','USER','dbo','TABLE','RA_OBLIG_TRIGGER';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS OBLIG TYPE: Use this table to record all possible names, codes and other identifiers for the type of obligation incurred, such as termination notice, surrendor notice, annual rental.','USER','dbo','TABLE','RA_OBLIG_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS OBLIGATION CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the reason the obligations have been related to each other, such as a lease rental payment that has been allocated to its subordinatelease components.','USER','dbo','TABLE','RA_OBLIG_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS OFFSHORE AREA CODE: Use this table to record all possible names, codes and other identifiers for valid offshore area codes used for US offshore well locations.','USER','dbo','TABLE','RA_OFFSHORE_AREA_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS OFFSHORE COMPLETION TYPE: Use this table to record all possible names, codes and other identifiers for the location of the completion equipment on the drilling rig. For example, values for the offshore completion can be above water, under water etc.','USER','dbo','TABLE','RA_OFFSHORE_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS OIL VALUE CODE: Use this table to record all possible names, codes and other identifiers for the code assigned to the analysis oil by observation, in cases where numeric values are not used.','USER','dbo','TABLE','RA_OIL_VALUE_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ONTOGENY TYPE: Use this table to record all possible names, codes and other identifiers for the early life history of an organism, i.e., the subsequent stages it passes from the zygote to the mature adult.','USER','dbo','TABLE','RA_ONTOGENY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS OPERAND QUALIFIER: Use this table to record all possible names, codes and other identifiers for the symbols used to qualify the measurement values. For example, the symbol (>) indicates the value is more than the displayed measurement. Similarly the symbol(<) indicates the v alue is less than the displayed measurement. The symbols can include all the mathematical operands (<,>,+,=,-).','USER','dbo','TABLE','RA_OPERAND_QUALIFIER';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ORIENTATION: Use this table to record all possible names, codes and other identifiers for valid orientations of measurements to reference lines. For example, parallel, perpendicular, ...','USER','dbo','TABLE','RA_ORIENTATION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MACERAL AMOUNT: Use this table to record all possible names, codes and other identifiers for a description of the amount of maceral (trace, abundant). This is often a name that relates to a range of values, such as rare = <.1%. This is always going to be liptinite. Do not use ORGANIC MATTER TYPE (function is unclear).If used in petrology table, the meaning would need to be organic matter in coal or organic matter that is dispersedthrough the rocks or both. (Coal vs DOM dispersed organic matter - vs both). Check the Petrology table tobe sure we do properly.','USER','dbo','TABLE','RA_PALEO_AMOUNT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PALEO INDICATOR TYPE: Use this table to record all possible names, codes and other identifiers for a set of indicator types typically generated during fossil analysis and interpretation. Can inlude youngest, oldest, deepest, reworked, out of place, etc.','USER','dbo','TABLE','RA_PALEO_IND_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PALEO INTERPRETATION TYPE: Use this table to record all possible names, codes and other identifiers for the type of interpreted inforamtion contained, such as an age boundary, change boundary, age zone etc.','USER','dbo','TABLE','RA_PALEO_INTERP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PALEO FOSSIL TYPE: Use this table to record all possible names, codes and other identifiers for the type of type fossil identified such as holotype - first documented occurrence of this fossil in the literature, Paratype - when you add detail from other specimens, neotype - when the holotype has been lost and this is a replacement for study.','USER','dbo','TABLE','RA_PALEO_TYPE_FOSSIL';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PALEO SUMMARY COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a paleological summary','USER','dbo','TABLE','RA_PAL_SUM_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PALEO SUMMARY CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of cross reference between paleo summaries. Could include reports that are included in another, reports that supplement or replace others or reports that are simply different versions (although we dont recommend that you store draft copies in your database).','USER','dbo','TABLE','RA_PAL_SUM_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PARCEL LOT TYPE: Use this table to record all possible names, codes and other identifiers for the type of parcel lot descibed. Used to describe the type of US land parcel lots, in the congressional legal survey system etc..','USER','dbo','TABLE','RA_PARCEL_LOT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PARCEL TYPE: Use this table to record all possible names, codes and other identifiers for the type of land parcel. May be one of: Congressional, Carter, DLS, FPS, Geodetic, NE Loc, North sea, NTS, Offshore, Ohio, Texas.','USER','dbo','TABLE','RA_PARCEL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PAYMENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of payment that is made, such as rental, royalty. If the payment is a rental payment, the kind of rental payment is found in R LAND RENTAL TYPE. Type of royalty payments are captures in R ROYALTY TYPE.','USER','dbo','TABLE','RA_PAYMENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PAYZONE TYPE: Use this table to record all possible names, codes and other identifiers for the type of payzone either production or pay.','USER','dbo','TABLE','RA_PAYZONE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PAYMENT DETAIL TYPE: Use this table to record all possible names, codes and other identifiers for the Payment Detail Type, this may be tax, bank service charge, lessor payment.','USER','dbo','TABLE','RA_PAY_DETAIL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PAYMENT METHOD: Use this table to record all possible names, codes and other identifiers for the Payment Method such as direct deposit or cheque.','USER','dbo','TABLE','RA_PAY_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PAYMENT RATE TYPE: Use this table to record all possible names, codes and other identifiers for the Payment Rate Type such as tax or rental.','USER','dbo','TABLE','RA_PAY_RATE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PDEN AMENDMENT REASON: Use this table to record all possible names, codes and other identifiers for the reason why a production amendment was posted, such as instrument calibration, calculation error or volume balancing.','USER','dbo','TABLE','RA_PDEN_AMEND_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PRODUCTION ENTITY COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a production entity.','USER','dbo','TABLE','RA_PDEN_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PDEN STATUS: Use this table to record all possible names, codes and other identifiers for the state of the production entity from the point of view described in PDEN STATUS TYPE (such as operational status).','USER','dbo','TABLE','RA_PDEN_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PDEN STATUS TYPE: Use this table to record all possible names, codes and other identifiers for the type of status, such as the operational status, the financial status, the legal status, the eligibility status etc.','USER','dbo','TABLE','RA_PDEN_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PRODUCTION REPORTING ENTITY CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of cross reference. Used in situations where you may want two different XREF networks (ownership and physical connections like pipelines, for instance).','USER','dbo','TABLE','RA_PDEN_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PERFORATION METHOD: Use this table to record all possible names, codes and other identifiers for the type of opening the fluid entered through into the tubing (e.g., perforation, open hole, combination, etc.).','USER','dbo','TABLE','RA_PERFORATION_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PERFORATION TYPE: Use this table to record all possible names, codes and other identifiers for the type of perforation method. For example bullet, jet or combination.','USER','dbo','TABLE','RA_PERFORATION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PERIOD TYPE: Use this table to record all possible names, codes and other identifiers for the periods of time. Possible values include DAY, MONTH, YEAR.','USER','dbo','TABLE','RA_PERIOD_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PERMEABILITY TYPE: Use this table to record all possible names, codes and other identifiers for the ability of a rock body to transmit fluids, typically measured in darcies or millidarcies. Formations that transmit fluids readily, such as sandstones, are described as permeable and tend to have many large, well-connected pores. Impermeable formations, such as shales and siltstones, tend to be finer grained or of a mixed grain size, with smaller, fewer, or less interconnected pores. This is intended to serve as a qualitative value, rather than measured values.','USER','dbo','TABLE','RA_PERMEABILITY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PHYSICAL ITEM STATUS: Use this table to record all possible names, codes and other identifiers for the Physical Items Status, this may be available, lost, destroyed, unknown etc','USER','dbo','TABLE','RA_PHYSICAL_ITEM_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PHYSICAL PROCESS: Use this table to record all possible names, codes and other identifiers for the process used to create a new physical rendering of an item. May be tape copy, film copy ...','USER','dbo','TABLE','RA_PHYSICAL_PROCESS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PHYSICAL ITEM GROUP TYPE: Use this table to record all possible names, codes and other identifiers for the type of physical group created, such as a group of images that form a composite, or a montage, or a file set that are to be distributed together.','USER','dbo','TABLE','RA_PHYS_ITEM_GROUP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PICK LOCATION: Use this table to record all possible names, codes and other identifiers for the location or type of formation (strat unit) pick. For example top, base or middle.','USER','dbo','TABLE','RA_PICK_LOCATION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PICK QUALIFIER: Use this table to record all possible names, codes and other identifiers for a qualifier used to describe a formation pick. For example not logged, missing, estimated or uncertain depth.','USER','dbo','TABLE','RA_PICK_QUALIFIER';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PICK QUALIFIER REASON: Use this table to record all possible names, codes and other identifiers for the reason that a qualifier was added for a pick, such as poor data, faulting, erosion.','USER','dbo','TABLE','RA_PICK_QUALIF_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PICK QUALITY: Use this table to record all possible names, codes and other identifiers for the quality of or degree of confidence in the pick that was made, such as poor, uncertain, good, excellent.','USER','dbo','TABLE','RA_PICK_QUALITY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PICK VERSION TYPE: Use this table to record all possible names, codes and other identifiers for the type of version of pick that is given in an interpretation table, such as working, final, proposed etc.','USER','dbo','TABLE','RA_PICK_VERSION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PLATFORM TYPE: Use this table to record all possible names, codes and other identifiers for a type of drilling platform or pad. For example, fixed platform, compliant tower, tension leg, or onshore pad.','USER','dbo','TABLE','RA_PLATFORM_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PLOT SYMBOL: Use this table to record all possible names, codes and other identifiers for the required information to represent a character or plot symbol. For example this may be particular mapping package coding scheme to represent well status codes a pointer to other application dependent files.','USER','dbo','TABLE','RA_PLOT_SYMBOL';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PLUG TYPE: Use this table to record all possible names, codes and other identifiers for the type of operation performed to plugback the well. For example, the type of plugback may be a temporary plugback with a retrievable bridge plug or a cement plug.','USER','dbo','TABLE','RA_PLUG_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS POOL COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a pool.','USER','dbo','TABLE','RA_POOL_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS POOL STATUS: Use this table to record all possible names, codes and other identifiers for the operational or legal status of the pool.','USER','dbo','TABLE','RA_POOL_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS POOL TYPE: Use this table to record all possible names, codes and other identifiers for the type of pool.','USER','dbo','TABLE','RA_POOL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS POROSITY TYPE: Use this table to record all possible names, codes and other identifiers for the type of porosity observed. For example intergranular, sucrosic or cavernous.','USER','dbo','TABLE','RA_POROSITY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM AUDIT REASON: Use this table to record all possible names, codes and other identifiers for the reason why an auditable change was made to the data, such as data cleanup project, new data received, incorrect data corrected, missing datalocated etc.','USER','dbo','TABLE','RA_PPDM_AUDIT_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM COLUMN AUDIT TYPE: Use this table to record all possible names, codes and other identifiers for the type of change that is being tracked in this row of audit data. Depending on the business rules in place, may track inserts, updates or deletes.','USER','dbo','TABLE','RA_PPDM_AUDIT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM BOOLEAN RULE: Use this table to record all possible names, codes and other identifiers for Boolean Rules. A boolean rule are part of a logical sytstem of rules, based on mathematical rules. These represent relationships between sets of values using logical operators such as AND, OR, NOT, GREATER THAN, LESS THAN etc. Named after the British mathemetician George Boole.','USER','dbo','TABLE','RA_PPDM_BOOLEAN_RULE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM COLUMN KEY METHOD TYPE: Use this table to record all possible names, codes and other identifiers for the type of method that is used to create the value in this column. The method could include manual selection and key entry, automated random generation, concatenation of values etc. If you wish, the code used to generate the key value can be stored in PPDM COLUMN, and the last assigned key can also be stored.','USER','dbo','TABLE','RA_PPDM_CREATE_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM COLUMN DATA TYPE: Use this table to record all possible names, codes and other identifiers for the database datatype that is assigned to a column or value. Usually, number, character, date etc.','USER','dbo','TABLE','RA_PPDM_DATA_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DEFAULT VALUE METHOD: Use this table to record all possible names, codes and other identifiers for the method used to assign a default value to this column, in the event that a default value is allowed. May include things like a SYSTEM DATE, USERID, null, or some other value. Please note that default values should be used with great caution and documentation of business rules. Default values can leave users confused, or may be misleading if they are not carefully implemented.','USER','dbo','TABLE','RA_PPDM_DEFAULT_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM RULE ENFORCE METHOD: Use this table to record all possible names, codes and other identifiers for the types of method that is used to enforce a rule, such as RDBMS business rule, stored procedure, function, software procedure, manual enforcement etc.','USER','dbo','TABLE','RA_PPDM_ENFORCE_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM RULE FAIL RESULT: Use this table to record all possible names, codes and other identifiers for the result if the enforcement of a rule fails, such as critical error, warning, notification etc.','USER','dbo','TABLE','RA_PPDM_FAIL_RESULT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM GROUP TYPE: Use this table to record all possible names, codes and other identifiers for the type of group that is being described. Could be an application group, query group, function group, module type etc.','USER','dbo','TABLE','RA_PPDM_GROUP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM GROUP USE: Use this table to record all possible names, codes and other identifiers for the function or useage of a table or column in a group. Examples include direct usage, a dependency, a core part of the group or a referenced sectionetc.','USER','dbo','TABLE','RA_PPDM_GROUP_USE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM GROUP CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of relationship between two groups, such as a hierarchical relationship between groups used for creating a report, replacements and other types of dependencies.','USER','dbo','TABLE','RA_PPDM_GROUP_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM INDEX CATEGORY: Use this table to record all possible names, codes and other identifiers for the technical category of the index, such as bit mapped or normal (regular).','USER','dbo','TABLE','RA_PPDM_INDEX_CATEGORY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM MAPPING RULE TYPE: Use this table to record all possible names, codes and other identifiers for the type of rule that is used for conditional mapping between systems, such as when the mapping depends on the value of the column, or thevalue of another related column.','USER','dbo','TABLE','RA_PPDM_MAP_RULE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM MAPPING TYPE: Use this table to record all possible names, codes and other identifiers for the type of mapping between two elements, such as data that is simply migrated from one system to the other without any change, data that is mapped through a reference table, a mapping that requires some computation or calculation or a multiple mapping where the source and target do not have a simple one to one relationship.','USER','dbo','TABLE','RA_PPDM_MAP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM METRIC CODE: Use this table to record all possible names, codes and other identifiers for a measurement or indicative code for a specific kind of metric. Some metrics may be measured quantitatively, through numbers, and others may be measured qualitatively, though more subjective values such as good, complete etc.','USER','dbo','TABLE','RA_PPDM_METRIC_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM METRIC COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component that is associated with the metric. Use this to relate the metrics to an overall project, or the tables and columns or schema that are associated with the metric.','USER','dbo','TABLE','RA_PPDM_METRIC_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM REFERENCE VALUE TYPE: Use this table to record all possible names, codes and other identifiers for the specific value that is being measured, such as the region or location for the value. For example, the metric may measure the number of wells in the database, but the reference value may further refine this to indicate the number of wells in the Northern region.','USER','dbo','TABLE','RA_PPDM_METRIC_REF_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM METRIC TYPE: Use this table to record all possible names, codes and other identifiers for the type of metric that is being measured, such the count of wells that have been quality controlled, the number of SW licenses that are in use, the time taken to complete an activity etc.','USER','dbo','TABLE','RA_PPDM_METRIC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM OBJECT STATUS: Use this table to record all possible names, codes and other identifiers for the current status of the object, such as enabled or disabled.','USER','dbo','TABLE','RA_PPDM_OBJECT_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM OBJECT TYPE: Use this table to record all possible names, codes and other identifiers for the type of database object that is being tracked, such as a table, column, index, constraint or procedure.','USER','dbo','TABLE','RA_PPDM_OBJECT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM OPERATING SYSTEM: Use this table to record all possible names, codes and other identifiers for the Operating System. Also known as an "OS," this is the software that communicates with computer hardware on the most basic level. Without an operating system, no software programs can run. The OS is what allocates memory, processes tasks, accesses disks and peripherials, and serves as the user interface. (Sharpened Glossary)','USER','dbo','TABLE','RA_PPDM_OPERATING_SYSTEM';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM OWNER ROLE: Use this table to record all possible names, codes and other identifiers for the roles that applications or business associates can play in the ownership of a group of tables and columns. For example, you may list the business value owner, the technical value owner, the data manager, the generating application, a using application and so on.','USER','dbo','TABLE','RA_PPDM_OWNER_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM PROCEDURE TYPE: Use this table to record all possible names, codes and other identifiers for the type of procedure, such as stored, called, function etc.','USER','dbo','TABLE','RA_PPDM_PROC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM QUALITY CONTROL QUALITY: Use this table to record all possible names, codes and other identifiers for that indicate the quality of the data, whether the data is poor quality, fully verified, falls within expected range of values, requires further investigation etc.','USER','dbo','TABLE','RA_PPDM_QC_QUALITY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM QUALITY CONTROL STATUS: Use this table to record all possible names, codes and other identifiers for a valid type of quality control or validation status, such as batch loaded, visual inspection, verified by data analyst, verified by business analyst etc.','USER','dbo','TABLE','RA_PPDM_QC_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM QUALITY CONTROL TYPE: Use this table to record all possible names, codes and other identifiers for the type of quality control that is being done, such as table level or column level quality control.','USER','dbo','TABLE','RA_PPDM_QC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM RELATIONAL DATA BASE MANAGEMENT SYSTEM: Use this table to record all possible names, codes and other identifiers for RDBMSs. RDBMS, short for relational database management system and pronounced as separate letters, a type of database management system (DBMS) that stores data in the form of related tables. Relational databases are powerful because they require few assumptions about how data is related or how it will be extracted from the database. As a result, the same database can be viewed in many different ways. An important featureof relational systems is that a single database can be spread across several tables. This differs from flat-file databases, in which each database is self-contained in a single table. (ISP Glossary)','USER','dbo','TABLE','RA_PPDM_RDBMS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM REFERENCE VALUE TYPE: Use this table to record all possible names, codes and other identifiers for the kinds of reference values that are used for declaration of business rules about data in your database. For example, you may statethat the reference value type is the value of another column (such as in the case where the spud date must be after the well license date), or the case where if one column is populated, another must also be populated (if a production volume is entered, you must also enter a unit of measure).','USER','dbo','TABLE','RA_PPDM_REF_VALUE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM ROW QUALITY:  Use this table to provide alternate codes for quality indicators on rows of data.','USER','dbo','TABLE','RA_PPDM_ROW_QUALITY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM RULE CLASS: Use this table to record all possible names, codes and other identifiers for the class or type of rule, such as policy, practice, procedure, business rule.','USER','dbo','TABLE','RA_PPDM_RULE_CLASS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM RULE COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component that is associated with the rule.','USER','dbo','TABLE','RA_PPDM_RULE_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM RULE DETAIL TYPE: Use this table to record all possible names, codes and other identifiers for the type of detail for a rule that is being captured, such as the minimum value that the column can store.','USER','dbo','TABLE','RA_PPDM_RULE_DETAIL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM RULE PURPOSE: Use this table to record all possible names, codes and other identifiers for the objective or purpose for the business rule, such as data load quality control, management reporting etc.','USER','dbo','TABLE','RA_PPDM_RULE_PURPOSE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM RULE STATUS: Use this table to record all possible names, codes and other identifiers for the current status of a business rule, such as proposed, under review, approved, published, deprecated etc.','USER','dbo','TABLE','RA_PPDM_RULE_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RULE USE CONDITION TYPE: Use this table to record all possible names, codes and other identifiers for the type of condition that must be met for this procedure to be run. For example, the procedure may be used during inserts to the well table, during updates, during migrations, run monthly etc.','USER','dbo','TABLE','RA_PPDM_RULE_USE_COND';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM RULE CROSS REFERENCE CONDITION: Use this table to record all possible names, codes and other identifiers for the type of condition that must be met before this cross reference comes into effect, usually used when you need to create a branched or dependent sequence of events. For example, one row of data will state that if the rule indicated as RULE_ID is successful, go to RULE_ID2. In another row of data, you can state that if the first rule has failed, a different RULE_ID2 would be in force.','USER','dbo','TABLE','RA_PPDM_RULE_XREF_COND';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM RULE CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the reason why a cross reference was created, such as an indication of a rule to be processed in the event that the first rule hasbeen processed and passed or failed. In this case, the reference XREF TYPE may be RUN RULE ID2 if RULE ID passes (or fails). May also be used for rule management, such as rule that replaces an old rule, rule that must be implemented before or after another rule a rule that is a component of another rule, or to build the relationships between policies, practices, procedures and business rules.','USER','dbo','TABLE','RA_PPDM_RULE_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM SCHEMA ENTITY TYPE: Use this table to record all possible names, codes and other identifiers for the type of schema element that is being described, such as element, attribute, element group etc.','USER','dbo','TABLE','RA_PPDM_SCHEMA_ENTITY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SCHEMA GROUP TYPE: Use this table to record all possible names, codes and other identifiers for the type of grouping of schema entities that has been created, such as the relationship between an element and its attributes, parent child relationships, siblings, sequencing elements.','USER','dbo','TABLE','RA_PPDM_SCHEMA_GROUP';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM SYSTEM TYPE: Use this table to record all possible names, codes and other identifiers for valid types of systems, such as Relational Database, XML Schema, EDI, etc.','USER','dbo','TABLE','RA_PPDM_SYSTEM_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TABLE TYPE: Use this table to record all possible names, codes and other identifiers for the type of entry in this table that describes the physical implementation, such as table, view, materialized view etc.','USER','dbo','TABLE','RA_PPDM_TABLE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM UNIT OF MEASURE ALIAS TYPE: Use this table to record all possible names, codes and other identifiers for the type of unit of measure alias or symbol that is referenced. In the sample data, the symbol types may be UTF8_SYMABOL, MIXED_CASE_SYMBOL, SINGLE_CASE_SYMBOL, PRINT_SYMBOL (used for representations that use special characters) or EPSG_SYMBOL (used for EPSG coordinate system references).','USER','dbo','TABLE','RA_PPDM_UOM_ALIAS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PPDM UNIT OF MEASURE USAGE: Use this table to record all possible names, codes and other identifiers for the valid types of usage values, usually as defined by an authorized agency such as IEEE. Usual values would be NULL or CURRENT for current, deprecated, discouraged or strongly discouraged. Note that different reference sources may supply different values for usage. For example, API RP66 shows the BAR as a current unit of measure while the SI-10 shows it as deprecated.','USER','dbo','TABLE','RA_PPDM_UOM_USAGE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PRESERVATION QUALITY TYPE: Use this table to record all possible names, codes and other identifiers for the quality of preservation for the samples used.','USER','dbo','TABLE','RA_PRESERVE_QUALITY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PRESERVATION TYPE: Use this table to record all possible names, codes and other identifiers for the type of preservation for a lithologic fossil, such as abraded, crushed, broken, pyritized etc.','USER','dbo','TABLE','RA_PRESERVE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PRODUCTION METHOD: Use this table to record all possible names, codes and other identifiers for the method of production. For example swabbing, flowing, pumping or gas lift.','USER','dbo','TABLE','RA_PRODUCTION_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PRODUCT COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a product.','USER','dbo','TABLE','RA_PRODUCT_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PRODUCTION STRING COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a production string.','USER','dbo','TABLE','RA_PROD_STRING_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PRODUCTION STRING STATUS: Use this table to record all possible names, codes and other identifiers for valid values for a production string status. This table allows you to capture status information from many points of view as it changesover time.','USER','dbo','TABLE','RA_PROD_STRING_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PRODUCTION STRING STATUS TYPE: Use this table to record all possible names, codes and other identifiers for the type of status reported for the production string. Can include construction status, operating status, producing status, abandonment status etc.','USER','dbo','TABLE','RA_PROD_STRING_STAT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PRODUCTION STRING TYPE: Use this table to record all possible names, codes and other identifiers for the type and/or status of the production string. The string could be abandoned, producing, disposal, injection, shut-in, etc..','USER','dbo','TABLE','RA_PROD_STRING_TYPE';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_PROD_STR_FM_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PRODUCTION STRING FORMATION STATUS TYPE: Use this table to record all possible names, codes and other identifiers for The type of status reported at the production string formation, such as completion status.','USER','dbo','TABLE','RA_PROD_STR_FM_STAT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PROJECTION TYPE: Use this table to record all possible names, codes and other identifiers for valid classifications of projections used by individual map projections. For example, Mercator, Lambert, Polyconic, ...','USER','dbo','TABLE','RA_PROJECTION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PROJECT ALIAS TYPE: Use this table to record all possible names, codes and other identifiers for the type of project alias that has been assigned. Could be the code assigned by an application or user, or by another organization.','USER','dbo','TABLE','RA_PROJECT_ALIAS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PROJECT BUSINESS ASSOCIATE ROLE: Use this table to record all possible names, codes and other identifiers for the role of the business associate in the project, such as project manager, technical lead, DBA etc.','USER','dbo','TABLE','RA_PROJECT_BA_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PROJECT COPMPONENT REASON: Use this table to record all possible names, codes and other identifiers for the reason why business objects or other entities are related to this project. This may occur when one project is part of another project, when wells, land rights or seismc are related to a project etc.','USER','dbo','TABLE','RA_PROJECT_COMP_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PROJECT COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the reason the component is associated with the project, such as created for, contract that governs, used during etc.','USER','dbo','TABLE','RA_PROJECT_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_PROJECT_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PROJECT EQUIPMENT ROLE: Use this table to record all possible names, codes and other identifiers for the role played by a piece of equipment in a project, such as pumper, saftey equipment, processor, primary storage device etc.','USER','dbo','TABLE','RA_PROJECT_EQUIP_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PROJECT or PROJECT STEP STATUS: Use this table to record all possible names, codes and other identifiers for the status of a project or a step in a project. May include underway, on hold, completed, cancelled.','USER','dbo','TABLE','RA_PROJECT_STATUS';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_PROJECT_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PROJECT TYPE: Use this table to record all possible names, codes and other identifiers for the type of project, such as seismic, geological, exploitation etc.','USER','dbo','TABLE','RA_PROJECT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PROJECT STEP TYPE: Use this table to record all possible names, codes and other identifiers for the type of step for the project.','USER','dbo','TABLE','RA_PROJ_STEP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PROJECT STEP CROSS REFERENCE REASON: Use this table to record all possible names, codes and other identifiers for relationships between steps in a project. May define necessary precursors, following steps, alternate paths etc.','USER','dbo','TABLE','RA_PROJ_STEP_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PROPPANT AGENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of proppant used in the hydraulic fracture treatment fluid. For example, fracturing sands, resin-coated ceramic proppants, sintered bauxites...','USER','dbo','TABLE','RA_PROPPANT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PUBLICATION NAME: Use this table to record all possible names, codes and other identifiers for the name of the publication that was referenced, such as the Oil and Gas Journal.','USER','dbo','TABLE','RA_PUBLICATION_NAME';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS QUALIFIER TYPE: Use this table to record all possible names, codes and other identifiers for the type of qualifier, relative to the qualifier described. Could be a diversity qualifier, species qualifier etc.','USER','dbo','TABLE','RA_QUALIFIER_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS QUALITY: Use this table to record all possible names, codes and other identifiers for standard quality indicators. For example poor, good, very good or excellent.','USER','dbo','TABLE','RA_QUALITY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RATE CONDITION: Use this table to record all possible names, codes and other identifiers for conditions that are applied to a rate schedule. For example a road access rate may only apply when a well is in production or during drilling operations.','USER','dbo','TABLE','RA_RATE_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RATE SCHEDULE TYPE: Use this table to record all possible names, codes and other identifiers for the type of schedule, such as a regulatory bodys lessor schedule for rental payments.','USER','dbo','TABLE','RA_RATE_SCHEDULE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RATE SCHEDULE COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a rate schedule.','USER','dbo','TABLE','RA_RATE_SCHEDULE_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RATE SCHEDULE CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the reason why the rate schedules have been cross referenced. A new schedule may replace an older one, or a supplementary schedule may be defined etc.','USER','dbo','TABLE','RA_RATE_SCHED_XREF';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RATE TYPE: Use this table to record all possible names, codes and other identifiers for the type of rate charged in this schedule. Examples may include posting fees, expenses, rentals etc.','USER','dbo','TABLE','RA_RATE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RATIO CURVE TYPE: Use this table to record all possible names, codes and other identifiers for the type of ratio curve that is used in decline curve forecast calculations such as linear, semi-log, log-log, etc.','USER','dbo','TABLE','RA_RATIO_CURVE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RATIO FLUID TYPE: Use this table to record all possible names, codes and other identifiers for the type of ratio fluid that is used in decline curve forecast calculations for GOR, YIELD, etc.','USER','dbo','TABLE','RA_RATIO_FLUID_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TEST RECORDER POSITION: Use this table to record all possible names, codes and other identifiers for the position of a test recorder or gauge relative to the test tool components. For example below or above straddle.','USER','dbo','TABLE','RA_RECORDER_POSITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TEST RECORDER TYPE: Use this table to record all possible names, codes and other identifiers for types or recorders or pressure gauges. For example bourbon tube, quartz gauge or strain gauge.','USER','dbo','TABLE','RA_RECORDER_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS REMARK TYPE: Use this table to record all possible names, codes and other identifiers for describing remark types. This is an open reference table commonly used to group remarks. For example drilling, geologists, regulatory or planning.','USER','dbo','TABLE','RA_REMARK_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS REMARK USE TYPE: Use this table to record all possible names, codes and other identifiers for the type of use that a remark should be put to, such as for internal use only, for external publication etc.','USER','dbo','TABLE','RA_REMARK_USE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS REPEAT STRATIGRAPHY TYPE: Use this table to record all possible names, codes and other identifiers for the reason the stratigraphic unit was picked twice in the same interpretation. Usually occurs because of horizontal or deviated drilling techniques or as a result of faulting or folding.','USER','dbo','TABLE','RA_REPEAT_STRAT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS HIERARCHY COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of business object that has been associated with a level in the hierarchy. Could be field, lease, area of interest, geographic area etc.','USER','dbo','TABLE','RA_REPORT_HIER_COMP';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS HIERARCHY LEVEL TYPE: Use this table to record all possible names, codes and other identifiers for the type of level that has been defined in the hierarchy, such as lease, pool etc.','USER','dbo','TABLE','RA_REPORT_HIER_LEVEL';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS HIERARCHY TYPE: Use this table to record all possible names, codes and other identifiers for the type of hierarchy that has been created.','USER','dbo','TABLE','RA_REPORT_HIER_TYPE';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_REP_HIER_ALIAS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RESERVE ENTITY COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a reserve entity','USER','dbo','TABLE','RA_RESENT_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RESERVE ENTITY CONFIDENCE TYPE: Use this table to record all possible names, codes and other identifiers for the level of confidence associated with this reserve class, such as proven, possible, probable.','USER','dbo','TABLE','RA_RESENT_CONFIDENCE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RESERVE ENTITY GROUPING CRITERIA TYPE: Use this table to record all possible names, codes and other identifiers for the criteria used to group reserve entities, such as aeria extent, lease based etc.','USER','dbo','TABLE','RA_RESENT_GROUP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS REVISION CATEGORY TYPE: Use this table to record all possible names, codes and other identifiers for the types of revision categories which have been defined. Used for grouping revision reasons into more generalized categories such as ADDITIONS and REVISIONS','USER','dbo','TABLE','RA_RESENT_REV_CAT';
go

execute sp_addextendedproperty 'Description','RESERVE ENTITY USE TYPE: Use this table to record all possible names, codes and other identifiers for the Reserve Entity Use Type. This differentiates between reserve classes where the client expects to enter data, i.e. Proved Developed Producing, from those reserve classes which are defined for reporting purposes only, i.e. Proved plus 1/2 Probable.','USER','dbo','TABLE','RA_RESENT_USE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RESERVE ENTITY VOLUME METHOD: Use this table to record all possible names, codes and other identifiers for the method used by the analyst to establish reserve volumes, such as decline analysis, material balance or volumetric calculations.','USER','dbo','TABLE','RA_RESENT_VOL_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RESERVE ENTITY CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of relationships that exists between two reserve entities. Examples are contains, replaces, adjacent etc.','USER','dbo','TABLE','RA_RESENT_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RESTRICTED ACTIVITY: Use this table to record all possible names, codes and other identifiers for the activity that is restricted, such as on land conventional operations, drilling etc.','USER','dbo','TABLE','RA_REST_ACTIVITY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RESTRICTION DURATION TYPE: Use this table to record all possible names, codes and other identifiers for whether the restriction is active for durations that are seasonal, permanent, variable, event driven, date driven etc.','USER','dbo','TABLE','RA_REST_DURATION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND RESTRICTION REMARK: Use this table to record all possible names, codes and other identifiers for Remarks about a land restriction that have been coded can be entered using this reference table. This allows regulatory agencies to list which remarks are relevant to the restriction.','USER','dbo','TABLE','RA_REST_REMARK';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND RESTRICTION TYPE: Use this table to record all possible names, codes and other identifiers for whether the restriction is on the surface, for minerals etc.','USER','dbo','TABLE','RA_REST_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RETENTION PERIOD: Use this table to record all possible names, codes and other identifiers for the length of time records or materials should be kept in a certain location or form for administrative, legal, fiscal, historical, or other purposes. Retention periods are determined by balancing the potential value of the information to the agency against the costs of storing the records containing that information. Retention periods are often set for record series, but specific records within that series may need to be retained longer because they are required for litigation or because circumstances give those records unexpected archival value.','USER','dbo','TABLE','RA_RETENTION_PERIOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS REVISION METHOD TYPE: Use this table to record all possible names, codes and other identifiers for the method used to calculate the revised volumes, such as decline analysis, materials balance, volumetric analysis etc.','USER','dbo','TABLE','RA_REVISION_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BLOWOUT PREVENTOR TYPE: Use this table to record all possible names, codes and other identifiers for the Blowout Preventor Type. This is a large valve at the top of a well that may be closed if the drilling crew loses control of formation fluids. By closing this valve (usually operated remotely via hydraulic actuators), the drilling crew usually regains control of the reservoir, and procedures can then be initiated to increase the mud density until it is possible to open the BOP and retain pressure control of the formation. BOPs come in a variety of styles, sizes and pressure ratings. Some can effectively close over an open wellbore, some are designed to seal around tubular components in the well (drillpipe, casing or tubing) and others are fitted with hardened steel shearing surfaces that can actually cut through drillpipe. Since BOPs are critically important to the safety of the crew, the rig and the wellbore itself, BOPs are inspected, tested and refurbished at regular intervals determined by a combination of risk assessment, local practice, well type and legal requirements. BOP tests vary from daily function testing on critical wells to monthly or less frequent testing on wells thought to have low probability of well control problems. (Schlumberger Oilfield Glossary). Usual values include single, double, shear, ram.','USER','dbo','TABLE','RA_RIG_BLOWOUT_PREVENTER';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RIG CATEGORY: Use this table to record all possible names, codes and other identifiers for the category of the rig, describing its basic purpose. Typical examples include drilling rig, completion rig, service rig, wireline rig, workover rig, rathole rig etc.','USER','dbo','TABLE','RA_RIG_CATEGORY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RIG CLASS: Use this table to record all possible names, codes and other identifiers for the class of rig, such as single, super single, double, triple.','USER','dbo','TABLE','RA_RIG_CLASS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RIG CODE: Use this table to record all possible names, codes and other identifiers for the unique codes assigned to each rig or installation. For example, the code "GA" may be assigned to the drilling rig Glomar Arctic III.','USER','dbo','TABLE','RA_RIG_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RIG DESANDER TYPE: Use this table to record all possible names, codes and other identifiers for Rig Desander Types. These are a hydrocyclone device that removes large drill solids from the whole mud system. The desander should be located downstream of the shale shakers and degassers, but before the desilters or mud cleaners. A volume of mud is pumped into the wide upper section of the hydrocylone at an angle roughly tangent to its circumference. As the mud flows around and gradually down the inside of the cone shape, solids are separated from the liquid by centrifugal forces. The solids continue around and down until they exit the bottom of the hydrocyclone (along with small amounts of liquid) and are discarded. The cleaner and lighter density liquid mud travels up through a vortex in the center of the hydrocyclone, exits through piping at the top of the hydrocyclone and is then routed to the mud tanks and the next mud-cleaning device, usually a desilter. Various size desander and desilter cones are functionally identical, with the size of the cone determining the size of particles the device removes from the mud system. (Schlumberger Oilfield Glossary).','USER','dbo','TABLE','RA_RIG_DESANDER_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RIG DESILTER TYPE: Use this table to record all possible names, codes and other identifiers for the Rig Desilter Type. These are a hydrocyclone much like a desander except that its design incorporates a greater number of smaller cones. As with the desander, its purpose is to remove unwanted solids from the mud system. The smaller cones allow the desilter to efficiently remove smaller diameter drill solids than a desander does. For that reason, the desilter is located downstream from the desander in the surface mud system. (Schlumberger Oilfield Glossary).','USER','dbo','TABLE','RA_RIG_DESILTER_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RIG DRAWWORKS TYPE: Use this table to record all possible names, codes and other identifiers for the machine on the rig consisting of a large-diameter steel spool, brakes, a power source and assorted auxiliary devices. The primary function of the drawworks is to reel out and reel in the drilling line, a large diameter wire rope, in a controlled fashion. The drilling line is reeled over the crown block and traveling block to gain mechanical advantage in a "block and tackle" or "pulley" fashion. This reeling out and in of the drilling line causes the traveling block, and whatever may be hanging underneath it, to be lowered into or raised out of the wellbore. The reeling out of the drilling line is powered by gravity and reeling in by an electric motor or diesel engine.(Schlumberger Oilfield Glossary).','USER','dbo','TABLE','RA_RIG_DRAWWORKS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS GENERATOR or PLANT TYPE: Use this table to record all possible names, codes and other identifiers for the type of generator or plant, such as lighting or compressor.','USER','dbo','TABLE','RA_RIG_GENERATOR_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS HOOK or HOOKBLOCK TYPE: Use this table to record all possible names, codes and other identifiers for the high-capacity J-shaped equipment used to hang various other equipment, particularly the swivel and kelly, the elevator bails or topdrive units. The hook is attached to the bottom of the traveling block and provides a way to pick up heavy loads with the traveling block. The hook is either locked (the normal condition) or free to rotate, so that it may be mated or decoupled with items positioned around the rig floor, not limited to asingle direction. (Schlumberger Oilfield Glossary)','USER','dbo','TABLE','RA_RIG_HOOKBLOCK_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MAST TYPE: Use this table to record all possible names, codes and other identifiers for the structure used to support the crown blocks and the drillstring. Masts are usually rectangular or trapezoidal in shape and offer a very good stiffness, important to land rigs whose mast is laid down when the rig is moved. They suffer from being heavier than conventional derricks and consequently are not usually found in offshore environments, where weight is more of a concern than in land operations. (Schlumberger Oilfield Glossary).','USER','dbo','TABLE','RA_RIG_MAST';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RIG OVERHEAD EQUIPMENT CAPACITY TYPE: Use this table to record all possible names, codes and other identifiers for the type of capacity for the overhead equipment, such as static or dynamic.','USER','dbo','TABLE','RA_RIG_OVERHEAD_CAPACITY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RIG OVERHEAD EQUIPMENT TYPE: Use this table to record all possible names, codes and other identifiers for the class or type over overhead equipment, such as travelling block or swivel.','USER','dbo','TABLE','RA_RIG_OVERHEAD_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RIG PUMP TYPE: Use this table to record all possible names, codes and other identifiers for the pump on a rig is used to circulate materials in the well bore. Use this table to describe the type of pump.','USER','dbo','TABLE','RA_RIG_PUMP';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RIG PUMP FUNCTION: Use this table to record all possible names, codes and other identifiers for the function filled by the pump.','USER','dbo','TABLE','RA_RIG_PUMP_FUNCTION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RIG SUBSTRUCTURE: Use this table to record all possible names, codes and other identifiers for the foundation on which the derrick and engines sit. Contains space for storage and well control equipment. (http://www.oilonline.com/info/glossary.asp)','USER','dbo','TABLE','RA_RIG_SUBSTRUCTURE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RIG SWIVEL TYPE: Use this table to record all possible names, codes and other identifiers for a mechanical device that must simultaneously suspend the weight of the drillstring, provide for rotation of the drillstring beneath it while keepingthe upper portion stationary, and permit high-volume flow of high-pressure drilling mud from the fixed portion to the rotating portion without leaking. (Schlumberger Oilfield Glossary)','USER','dbo','TABLE','RA_RIG_SWIVEL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RIG TYPE: Use this table to record all possible names, codes and other identifiers for the type of rig installation. For example land, barge, submersible, platform, jackup, drillship, semisubmersible or artificial gravel island...','USER','dbo','TABLE','RA_RIG_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RECORDS MANAGEMENT INFORMATION ITEM CONTACT TYPE: Use this table to record all possible names, codes and other identifiers for the type of contact for an information item, other than authorship. For example, you may need to capture the contact for purchase, the contact for authorization to access the information or the contact for obtaining updates.','USER','dbo','TABLE','RA_RMII_CONTACT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RECORDS MANAGEMENT INFORMATION ITEM CONTENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of content associated with a records management information item','USER','dbo','TABLE','RA_RMII_CONTENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RECORDS MANAGEMENT INFORMATION ITEM DEFICIENCY TYPE: Use this table to record all possible names, codes and other identifiers for the types of deficiencies that may be noted on an information item.','USER','dbo','TABLE','RA_RMII_DEFICIENCY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RECORDS MANAGEMENT INFORMATION ITEM DESCRIPTION TYPE: Use this table to record all possible names, codes and other identifiers for the type of description for the information item. Could be a scale reference, type of report, classification, size or anything needed.','USER','dbo','TABLE','RA_RMII_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RECORDS MANAGEMENT INFORMATION ITEM GROUP TYPE: Use this table to record all possible names, codes and other identifiers for the type of information item group, such as a well file, that consists of a set of documents or other products which together serve a useful business function. If you use the Dublin Core terms, values would be conformsTo, hasFormat, hasPart, hasVersion, isFormatOf, isPartOf, isReferencedBy, isReplacedBy, isRequiredBy, isVersionOf, references, replaces, requires.','USER','dbo','TABLE','RA_RMII_GROUP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE RECORDS MANAGEMENT INFORMATION ITEM META DATA CODE: Use this table to record all possible names, codes and other identifiers for meta data values, often set by the creators of a meta data standard.','USER','dbo','TABLE','RA_RMII_METADATA_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RECORDS MANAGEMENT INFORMATION ITEM METADATA CODE TYPE: Use this table to record all possible names, codes and other identifiers for the category or type of information, as defined in meta data (such as Dublin Core, FGDC or ISO) that is being stored.','USER','dbo','TABLE','RA_RMII_METADATA_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RECORDS MANAGEMENT INFORMATION ITEM QUALITY CODE: Use this table to record all possible names, codes and other identifiers for the quality of the information contained on the information item.','USER','dbo','TABLE','RA_RMII_QUALITY_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RECORDS MANAGEMENT INFORMATION ITEM STATUS: Use this table to record all possible names, codes and other identifiers for a specific STATUS TYPE, the status of an information item. The status of information may differ from the status of its physical representation in some cases, such as information that is subject to updates or renewals.','USER','dbo','TABLE','RA_RMII_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RECORDS MANAGEMENT INFORMATION ITEM STATUS TYPE: Use this table to record all possible names, codes and other identifiers for the type of status that is being defined, such as the purchase order status for a subscription, the renewal status,or the approval status of the information.','USER','dbo','TABLE','RA_RMII_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS THESAURUS CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the relationship between words in two thesaurii, such as replaces previous value, exact semantic meaning, sub type of semantic meaning, super type of semantic meaning, partial overlap in meaning, completely different meanings (use when the words are the same, but the semantic does not correrspond).','USER','dbo','TABLE','RA_RM_THESAURUS_XREF';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ROAD CONDITION: Use this table to record all possible names, codes and other identifiers for road conditions. Road conditions provide important environmental, safety and billing information. A sample list of values may be found at http://www.highways.gov.sk.ca/docs/travelers_info/road_terminology.asp','USER','dbo','TABLE','RA_ROAD_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ROAD CONTROL TYPE: Use this table to record all possible names, codes and other identifiers for the type of controls in place to access a road, such as radio controls for well site access roads. Often, this is done for safety reasons.','USER','dbo','TABLE','RA_ROAD_CONTROL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ROAD DRIVING SIDE: Use this table to record all possible names, codes and other identifiers for the side of the road that you drive on, either left (Canada, US, Europe) or right (UK, Australia, Japan).','USER','dbo','TABLE','RA_ROAD_DRIVING_SIDE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ROAD TRAFFIC FLOW TYPE: Use this table to record all possible names, codes and other identifiers for the type of traffic flow for a road, usually one way, two way or reversible (traffic flow changes during the day or based on some otherbusiness rule).','USER','dbo','TABLE','RA_ROAD_TRAFFIC_FLOW_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ROCK CLASSIFICATION SCHEME: Use this table to record all possible names, codes and other identifiers for the rock classification scheme used to name the rock type. For example, the Folk classification scheme uses the terms, arkose, quartzarenite, etc. The Dunham classification scheme uses the terms mudstone, wackestone, etc.','USER','dbo','TABLE','RA_ROCK_CLASS_SCHEME';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ROLL ALONG METHOD: Use this table to record all possible names, codes and other identifiers for the type of roll along used for field recording. May be 4 station switch, fixed for patch, fixed for seis set etc.','USER','dbo','TABLE','RA_ROLL_ALONG_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ROYALTY TYPE: Use this table to record all possible names, codes and other identifiers for a type of royalty, such as gross overriding, net overriding, net profit interest, net carried interest. A royalty is a payment made to a party or a jurisdiction according to the terms of an agreement.','USER','dbo','TABLE','RA_ROYALTY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SALINITY TYPE: Use this table to record all possible names, codes and other identifiers for the type of water salinity measurement techniques used for well tests and sample analysis. For example chlorides or total dissolved solids.','USER','dbo','TABLE','RA_SALINITY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SAMPLE ANALYSIS COLLECTION TYPE: Use this table to record all possible names, codes and other identifiers for the type of collection reason used for gathering this sample, such as for daily tests, weekly tests, spot analysis etc.','USER','dbo','TABLE','RA_SAMPLE_COLLECTION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SAMPLE COLLECTION METHOD: Use this table to record all possible names, codes and other identifiers for the valid methods used to collect samples, such as coring or sidewall coring, dipping, cutting collection etc.','USER','dbo','TABLE','RA_SAMPLE_COLLECT_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SAMPLE COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a sample.','USER','dbo','TABLE','RA_SAMPLE_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SAMPLE CONTAMINANT: Use this table to record all possible names, codes and other identifiers for a contaminant in a sample that may affect later analysis. Drilling mud may be considered to be a contaminant.','USER','dbo','TABLE','RA_SAMPLE_CONTAMINANT';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_SAMPLE_DESC_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DESCRIPTION TYPE: Use this table to record all possible names, codes and other identifiers for the type of description for the sample that is not included in the SAMPLE table, the SAMPLE LITH DESC table or another table in the model. This table will support additional, less common description types.','USER','dbo','TABLE','RA_SAMPLE_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SAMPLE FRACTION TYPE: Use this table to record all possible names, codes and other identifiers for the type of fraction method used to separate the sample into two or more samples during this step. This process could involve phase or chemical separation. A homogeneous separation is possible in cases where the sample is to be subject to different kinds of analysis.','USER','dbo','TABLE','RA_SAMPLE_FRACTION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SAMPLE LOCATION: Use this table to record all possible names, codes and other identifiers for the various locations that samples can be extracted from. For example shaker, mud pit, core or wellbore.','USER','dbo','TABLE','RA_SAMPLE_LOCATION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SAMPLE PHASE TYPE: Use this table to record all possible names, codes and other identifiers for the phase of a sample, usually solid, gas or liquid. The phase of a sample may change from its original location as it is collected, stored and analyzed (e.g. methane, ethane, propane). Note: During well depletion the pressure will drop which can result in a phase change, this is a phase change at collection. C1 to C3 hydrocarbons may phase change during storage if the temperature and pressure are different from that of the well, this is phase change at storage. Pyrolysis results in a phase change of the sample during analysis.','USER','dbo','TABLE','RA_SAMPLE_PHASE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SAMPLE PREPARATION CLASS: Use this table to record all possible names, codes and other identifiers for the type or class of preparation for the sample, such as a chemical wash, thin section, acid wash etc. Specific methods are stored in ANALYSIS_STEP.','USER','dbo','TABLE','RA_SAMPLE_PREP_CLASS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SAMPLE REFERENCE VALUE TYPE: Use this table to record all possible names, codes and other identifiers for the kind of value that is used to compare a description to. For example, could be a color when originally collected (reference value type is ORIGINAL COLLECTION TIME) , the degree of soil compaction at various collection depths (reference value type is SAMPLE COLLECTION DEPTH) etc.','USER','dbo','TABLE','RA_SAMPLE_REF_VALUE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SAMPLE SHAPE: Use this table to record all possible names, codes and other identifiers for the shape of the sample, such as cylindrical, square, oblong, amorphous, rectangular, slice, random etc.','USER','dbo','TABLE','RA_SAMPLE_SHAPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SAMPLE TYPE: Use this table to record all possible names, codes and other identifiers for the type of sample that is described. For example, a cutting.','USER','dbo','TABLE','RA_SAMPLE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SCALE TRANSFORM TYPE: Use this table to record all possible names, codes and other identifiers for the type of scaling transform. For example, linear, log, compressed or hybrid.','USER','dbo','TABLE','RA_SCALE_TRANSFORM';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SCREEN LOCATION: Shakers typically contain three or more screens, each of which progressively has a smaller mesh size. Use this table to record all possible names, codes and other identifiers for the relative position of each screen in the shaker, usually top, middle or bottom.','USER','dbo','TABLE','RA_SCREEN_LOCATION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SECTION TYPE: Use this table to record all possible names, codes and other identifiers for valid types of section or equivalent blocks. For example, block, bay, survey, militia donation, Michigan road land section, ...','USER','dbo','TABLE','RA_SECTION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC PATH: Use this table to record all possible names, codes and other identifiers for whether the path measured is one way or two way.','USER','dbo','TABLE','RA_SEISMIC_PATH';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC THREE DIMENSION TYPE: Use this table to record all possible names, codes and other identifiers for the type of 3D that has been completed, such as broadside, side shoot, inline etc.','USER','dbo','TABLE','RA_SEIS_3D_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC ACTIVITY CLASS: Use this table to record all possible names, codes and other identifiers for a class or group of activity types related to seismic data. Examples include planning activities, acquisition activities, processing activities, records management activities etc.','USER','dbo','TABLE','RA_SEIS_ACTIVITY_CLASS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC ACTIVITY TYPE: Use this table to record all possible names, codes and other identifiers for a kind of activity related to seismic data. Activites are qualified by the class of activity (R SEIS ACTIVITY CLASS) they are engaged in, such as planning activities, acquisition activities, processing activities, records management activities etc.','USER','dbo','TABLE','RA_SEIS_ACTIVITY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC AUTHORIZE LIMITATION: Use this table to record all possible names, codes and other identifiers for the limitation that is associated with the seismic authorization. Typical examples would include - requires chief geophysicists signature, no limitation.','USER','dbo','TABLE','RA_SEIS_AUTHORIZE_LIMIT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC AUTHORIZATION REASON: Use this table to record all possible names, codes and other identifiers for the reason why this authorization was granted, such as legislated, area not of interest, old data etc.','USER','dbo','TABLE','RA_SEIS_AUTHORIZE_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC AUTHORIZATION TYPE: Use this table to record all possible names, codes and other identifiers for the type of authorization that is granted. Examples are: all data, raw data only, paper copies of all data, to pre-processing only etc.','USER','dbo','TABLE','RA_SEIS_AUTHORIZE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BIN METHOD: Use this table to record all possible names, codes and other identifiers for the method used to create seismic bins, such as rectilinear, flex binning with radius of 4 bins, bin borrowing etc.','USER','dbo','TABLE','RA_SEIS_BIN_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS BIN OUTLINE TYPE: Use this table to record all possible names, codes and other identifiers for the type of outline described, such as the outline to the extent of partial coverage, or the outline to the extent of full coverage only.','USER','dbo','TABLE','RA_SEIS_BIN_OUTLINE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CABLE MAKE: Use this table to record all possible names, codes and other identifiers for the name and model of the cable that was used in the streamer.','USER','dbo','TABLE','RA_SEIS_CABLE_MAKE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CHANNEL TYPE: Use this table to record all possible names, codes and other identifiers for the type of channel that is used for recording the seismic data. May be time break channel, uphole channel, data channel etc.','USER','dbo','TABLE','RA_SEIS_CHANNEL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DIMENSION: Use this table to record all possible names, codes and other identifiers for the dimension or geometry of the seismic data. Mmay be 1D, 2D, 3D, swath, 3D water bottom.','USER','dbo','TABLE','RA_SEIS_DIMENSION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC ENERGY TYPE: Use this table to record all possible names, codes and other identifiers for describes the type of seismic energy source used.','USER','dbo','TABLE','RA_SEIS_ENERGY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC POINT FLOW DESCRIPTION TYPE: Use this table to record all possible names, codes and other identifiers for the type of remark made about a flowing seismic hole. Typical examples include description of the flow, description of remedial actions, description of the damage.','USER','dbo','TABLE','RA_SEIS_FLOW_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC GROUP TYPE: Use this table to record all possible names, codes and other identifiers for the type of group that is created. Groups can be created to associate a survey with the 2D or 3D sets acquired with it, to indicate what sets are combined into a processing or interepretation data set or to associate lines and segment','USER','dbo','TABLE','RA_SEIS_GROUP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC INSPECTION COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a seismic inspection.','USER','dbo','TABLE','RA_SEIS_INSP_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC LICENSE CONDITION TYPE: Use this table to record all possible names, codes and other identifiers for the type of condition that is applied to a seismic or geophysical license. Could include stipulations about disposal, timber salvage, inspections, compliance with legislation etc.','USER','dbo','TABLE','RA_SEIS_LIC_COND';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC LICENSE CONDITION CODE: Use this table to record all possible names, codes and other identifiers for a validated set of codes for a condition type.','USER','dbo','TABLE','RA_SEIS_LIC_COND_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DUE CONDITION: Use this table to record all possible names, codes and other identifiers for the state that must be achieved for the condition to become effective. For example, a report may be due 60 days after operations commence (or cease).','USER','dbo','TABLE','RA_SEIS_LIC_DUE_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LICENSE VIOLATION RESOLUTION TYPE: Use this table to record all possible names, codes and other identifiers for the type of resolution to a violation of a license term, such as the payment of a fine or creation of new processes.','USER','dbo','TABLE','RA_SEIS_LIC_VIOL_RESOL';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS VIOLATION TYPE: Use this table to record all possible names, codes and other identifiers for the type of violation of a license that is being recorded. Can be as simple as failure to submit necessary reports or something more difficult such as improper procedures.','USER','dbo','TABLE','RA_SEIS_LIC_VIOL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC PARAMETER ORIGIN: Use this table to record all possible names, codes and other identifiers for the Seismic Parameter Origin.','USER','dbo','TABLE','RA_SEIS_PARM_ORIGIN';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PATCH TYPE: Use this table to record all possible names, codes and other identifiers for the type of seismic patch that was used in recording, such as split spread, end-on, grid etc.','USER','dbo','TABLE','RA_SEIS_PATCH_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC PICK METHOD: Use this table to record all possible names, codes and other identifiers for the method that was used to make the seismic picks, such as automatic picking, manual picking, semi-automated picking etc.','USER','dbo','TABLE','RA_SEIS_PICK_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PROCESSING COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of processing component, such as seismic line, velocity survey etc.','USER','dbo','TABLE','RA_SEIS_PROC_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC PROCESSING PARAMETER TYPE NAME: Use this table to record all possible names, codes and other identifiers for the type or name of the processing parameter applied at this step, such as filter, gain etc.','USER','dbo','TABLE','RA_SEIS_PROC_PARM';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PROCESSING SET TYPE: Use this table to record all possible names, codes and other identifiers for the type of processing set, such as 3D set, recon processing etc.','USER','dbo','TABLE','RA_SEIS_PROC_SET_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PROCESSING STATUS: Use this table to record all possible names, codes and other identifiers for the status of processing, either for the step or the complete processing step. Could be waiting for data, complete, cancelled etc.','USER','dbo','TABLE','RA_SEIS_PROC_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC PROCESS STEP NAME: Use this table to record all possible names, codes and other identifiers for valid values for seismic processing step names.','USER','dbo','TABLE','RA_SEIS_PROC_STEP_NAME';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC PROCESS STEP TYPE: Use this table to record all possible names, codes and other identifiers for the type of processing step completed, such as migration, stack, flattening etc.','USER','dbo','TABLE','RA_SEIS_PROC_STEP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS RECORDING FORMAT TYPE: Use this table to record all possible names, codes and other identifiers for the Recording Format Type. This may be analog, SEG B, SEG Y etc','USER','dbo','TABLE','RA_SEIS_RCRD_FMT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC RECORDING INSTRUMENTS MAKE: Use this table to record all possible names, codes and other identifiers for the make and model of the equipment used.','USER','dbo','TABLE','RA_SEIS_RCRD_MAKE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC RECEIVER ARRAY TYPE: Use this table to record all possible names, codes and other identifiers for the type of receiver array used.','USER','dbo','TABLE','RA_SEIS_RCVR_ARRY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC RECEIVER TYPE: Use this table to record all possible names, codes and other identifiers for the Seismic Receiver Type. This may be geophone, hydrophone','USER','dbo','TABLE','RA_SEIS_RCVR_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC RECORD TYPE: Use this table to record all possible names, codes and other identifiers for the type of seismic record, such as good record, bad data, test record etc.','USER','dbo','TABLE','RA_SEIS_RECORD_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC REFERENCE DATUM: Use this table to record all possible names, codes and other identifiers for the datum to which depths have been corrected. For marine recording, this may be Mean Sea Level (MSL).','USER','dbo','TABLE','RA_SEIS_REF_DATUM';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC REPORTED REFERENCE NUMBER TYPE: Use this table to record all possible names, codes and other identifiers for the type of reference number associated with the seismic item that is being catalogued, such as a shot point number, trace number or file number.','USER','dbo','TABLE','RA_SEIS_REF_NUM_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC SAMPLE TYPE: Use this table to record all possible names, codes and other identifiers for the type of sample that is captured in this product. Usually either time or depth.','USER','dbo','TABLE','RA_SEIS_SAMPLE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC SEGMENT REASON: Use this table to record all possible names, codes and other identifiers for the reason the segment was created. May be acquisition, processing, interpretation, data partnership, data sale or transaction.','USER','dbo','TABLE','RA_SEIS_SEGMENT_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC SET COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a seismic set.','USER','dbo','TABLE','RA_SEIS_SET_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC SPECTRUM TYPE: Use this table to record all possible names, codes and other identifiers for types of measurements taken. May be magnetic, electromagnetic, gravity, shearwave...','USER','dbo','TABLE','RA_SEIS_SPECTRUM_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC SOURCE ARRAY TYPE: Use this table to record all possible names, codes and other identifiers for the Source Array Type such as: linear, circular, or star.','USER','dbo','TABLE','RA_SEIS_SRC_ARRAY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC SOURCE INSTRUMENTS MAKE: Use this table to record all possible names, codes and other identifiers for the make and model of the equipment used.','USER','dbo','TABLE','RA_SEIS_SRC_MAKE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC STATION TYPE: Use this table to record all possible names, codes and other identifiers for the seismic station type such as CDP, source, receiver etc','USER','dbo','TABLE','RA_SEIS_STATION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC STATUS: Use this table to record all possible names, codes and other identifiers for the status of the seismic set, such as underway, complete. May also represent an ownership status (trade, proprietary, spec etc.)','USER','dbo','TABLE','RA_SEIS_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC STATUS TYPE: Use this table to record all possible names, codes and other identifiers for the type of status of the seismic set. Typical status might include ownership (trade, proprietary, spec shoot) or acquisition status (planning,approved, active, complete).','USER','dbo','TABLE','RA_SEIS_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC SUMMARY TYPE: Use this table to record all possible names, codes and other identifiers for the reason why or kind of seismic summary that has been created, such as a mapping summary, one based on CDP, one based on ownership or some kind of activity.','USER','dbo','TABLE','RA_SEIS_SUMMARY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC SWEEP TYPE: Use this table to record all possible names, codes and other identifiers for the Sweep Type, this may be linear, variable.','USER','dbo','TABLE','RA_SEIS_SWEEP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC TRANSACTION COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a seismic transaction','USER','dbo','TABLE','RA_SEIS_TRANS_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEND METHOD: Use this table to record all possible names, codes and other identifiers for the method used to send an object from a sender to a receiver. Could be by registered mail, courier, hand delivery, email etc.','USER','dbo','TABLE','RA_SEND_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SERVICE QUALITY: Use this table to record all possible names, codes and other identifiers for the quality of service provided by this BUSINESS ASSOCIATE, either for a specified address, a service or a service at an address.','USER','dbo','TABLE','RA_SERVICE_QUALITY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEVERITY: Use this table to record all possible names, codes and other identifiers for severity types used for a qualifier of lost circulation or water flow into the wellbore. For example minor or severe.','USER','dbo','TABLE','RA_SEVERITY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SUPPORT FACILITY AIRSTRIP TYPE: Use this table to record all possible names, codes and other identifiers for valid types of airstrips.','USER','dbo','TABLE','RA_SF_AIRSTRIP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SUPPORT FACILITY BRIDGE TYPE: Use this table to record all possible names, codes and other identifiers for valid types of bride, such as permanent steel, wood, winter ice etc.','USER','dbo','TABLE','RA_SF_BRIDGE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SUPPORT FACILITY COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a support facility.','USER','dbo','TABLE','RA_SF_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SUPPORT FACILITY DESCRIPTION TYPE: Use this table to record all possible names, codes and other identifiers for the type of description for a support facility, such as color, construction material etc.','USER','dbo','TABLE','RA_SF_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_SF_DESC_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SUPPORT FACILITY DOCK TYPE: Use this table to record all possible names, codes and other identifiers for a valid type of dock used in marine operations.','USER','dbo','TABLE','RA_SF_DOCK_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SUPPORT FACILITY ELECTRIC FACILITY TYPE: Use this table to record all possible names, codes and other identifiers for the type of electric facility, such as generator station or power pole.','USER','dbo','TABLE','RA_SF_ELECTRIC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SUPPORT FACILITY LANDING FACILITY or HELIPORT TYPE: Use this table to record all possible names, codes and other identifiers for the type of landing facility, such as airstrip or heliport. May be a helipad (A prepared area designated and usedfor takeoff and landing of helicopters. (Includes touchdown or hover point.) ) or a heliport (A facility designated for operating, basing, servicing, and maintaining helicopters).','USER','dbo','TABLE','RA_SF_LANDING_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS MAINTAINENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of maintenace that will be done on this support facility, such as repaving, painting, surfacing etc.','USER','dbo','TABLE','RA_SF_MAINTAIN_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SUPPORT FACILITY PAD TYPE: Use this table to record all possible names, codes and other identifiers for a valid type of drilling pad.','USER','dbo','TABLE','RA_SF_PAD_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SUPPORT FACILITY ROAD TYPE: Use this table to record all possible names, codes and other identifiers for a valid type of road, such as public, logging, private etc.','USER','dbo','TABLE','RA_SF_ROAD_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SUPPORT FACILITY STATUS: Use this table to record all possible names, codes and other identifiers for the status of the support facility, such as working, abandoned etc.','USER','dbo','TABLE','RA_SF_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SUPPORT FACILITY STATUS TYPE: Use this table to record all possible names, codes and other identifiers for the type of support facilty status, such as operational, construction status, regulatory status etc. Used to cagegorize status information.','USER','dbo','TABLE','RA_SF_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SUPPORT FACILITY SURFACE TYPE: Use this table to record all possible names, codes and other identifiers for a valid list of surface types for various support facility. May include concrete, asphalt, dirt, gravel, sand etc.','USER','dbo','TABLE','RA_SF_SURFACE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SUPPORT FACILITY TOWER TYPE: Use this table to record all possible names, codes and other identifiers for the type of tower, such as electrical, radio, microwave etc.','USER','dbo','TABLE','RA_SF_TOWER_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS VEHICLE TYPE: Use this table to record all possible names, codes and other identifiers for the type of vehicle, such as truck, car, van, minivan, motorcycle, ATV, ambulance, trailer, bus etc.','USER','dbo','TABLE','RA_SF_VEHICLE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS VESSEL ROLE: Use this table to record all possible names, codes and other identifiers for the specific role played by a vessel during an operation, such as seismic source creation, drilling, cleanup, supplies transportation, personelle transportation etc.','USER','dbo','TABLE','RA_SF_VESSEL_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS VESSEL TYPE: Use this table to record all possible names, codes and other identifiers for the type of marine vessel, such as seismic recording, drilling rig, surveying, passenger.','USER','dbo','TABLE','RA_SF_VESSEL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SUPPORT FACILITY CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for a type of relationship that exists between two support facilities. For examples, two roads may cross, a tower may exist on a road right of way, a bridge may be on a road and so on.','USER','dbo','TABLE','RA_SF_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SHOW TYPE: Use this table to record all possible names, codes and other identifiers for the hydrocarbons in the object being evaluated. For example, show types can include Asphaltic stain, Bleeding gas, Oil fluorescence.','USER','dbo','TABLE','RA_SHOW_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SHUTIN PRESSURE TYPE: Use this table to record all possible names, codes and other identifiers for the type of shutin pressure being measured. For example, Tubing pressure, Casing pressure or Bottom Hole pressure.','USER','dbo','TABLE','RA_SHUTIN_PRESS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SOURCE: Use this table to record all possible names, codes and other identifiers for R_SOURCE','USER','dbo','TABLE','RA_SOURCE';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_SOURCE_ORIGIN';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SPACING UNIT TYPE: Use this table to record all possible names, codes and other identifiers for the type of spacing unit. May include drilling, producing.','USER','dbo','TABLE','RA_SPACING_UNIT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SPATIAL DESCRIPTION COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a spatial description.','USER','dbo','TABLE','RA_SPATIAL_DESC_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS LAND LEGAL DESCRIPTION TYPE: Use this table to record all possible names, codes and other identifiers for the type of land legal description. May be for a land right, surface restriction, pool, field, spacing unit etc.','USER','dbo','TABLE','RA_SPATIAL_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SPATIAL CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the reason why the spatial descriptions are related. Could be a spatial overlap, a business relationship etc.','USER','dbo','TABLE','RA_SPATIAL_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SPATIAL POINT VERSTION TYPE: Use this table to record all possible names, codes and other identifiers for the type of version that this location version describes. Could be an original location, agreed location, alternate location, archived location etc.','USER','dbo','TABLE','RA_SP_POINT_VERSION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SPATIAL ZONE DEFINITION CROSS REFERENCE REASON: Use this table to record all possible names, codes and other identifiers for the reason why spatial zone definitions have been cross referenced. The most common (but not the only) reason is a replacement of a zone definition with a new one by a regulatory agency.','USER','dbo','TABLE','RA_SP_ZONE_DEFIN_XREF';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ZONE DERIVATION METHOD: Use this table to record all possible names, codes and other identifiers for the type of log on which the definition is based, such as Borehole compensated sonic, induction electric, induction gamma ray etc.','USER','dbo','TABLE','RA_SP_ZONE_DERIV';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS ZONE TYPE: Use this table to record all possible names, codes and other identifiers for the type of mineral zone definiton, such as zone definition or DRRZD (Deep rights reversion zone definition)','USER','dbo','TABLE','RA_SP_ZONE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STATUS GROUP: Use this table to record all possible names, codes and other identifiers for groups of status codes. For example status of oil producer, dual oil producer and pumping oil may be grouped together as OIL.','USER','dbo','TABLE','RA_STATUS_GROUP';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS PHYSICAL STORE STATUS: Use this table to record all possible names, codes and other identifiers for the current status of this physical data store, such as operating, closed, destroyed etc.','USER','dbo','TABLE','RA_STORE_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC ACQUISITION METHOD: Use this table to record all possible names, codes and other identifiers for the method that was used to arrive at the stratigraphic analysis of data. May include Biostratigraphy, Radiometric techniques, cuttings, cores, logs, seismic etc.','USER','dbo','TABLE','RA_STRAT_ACQTN_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC AGE METHOD: Use this table to record all possible names, codes and other identifiers for the method used to determine the age of this stratigraphic unit, such as radiometric, fossil analysis etc.','USER','dbo','TABLE','RA_STRAT_AGE_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC ALIAS TYPE: Use this table to record all possible names, codes and other identifiers for the type of stratigraphic alias, such as working, vendor, regulatory etc.','USER','dbo','TABLE','RA_STRAT_ALIAS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC COLUMN TYPE: Use this table to record all possible names, codes and other identifiers for the type of stratigraphic column, such asbiostratigraphic, lithostratigraphic, sequence stratigraphic,proposed bore hole section, type section or regional section.','USER','dbo','TABLE','RA_STRAT_COLUMN_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC COLUMN CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the reason why the stratigraphic columns have been cross refernced. A new column may replace on outdated version, for example.','USER','dbo','TABLE','RA_STRAT_COL_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC CORRELATION CRITERIA: Use this table to record all possible names, codes and other identifiers for the basis or context within which the correlation was made, such as porosity, permeability, biostratigraphic age, lithologiccharacteristics, log character etc.','USER','dbo','TABLE','RA_STRAT_CORR_CRITERIA';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC CORRELATION TYPE: Use this table to record all possible names, codes and other identifiers for the type of stratigraphic correlation, such as lithostratigraphic, biostratigraphic, chronostratigraphic or other.','USER','dbo','TABLE','RA_STRAT_CORR_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC DESCRIPTION TYPE: Use this table to record all possible names, codes and other identifiers for the type of descriptive information included, usually classified as found in a stratigraphic lexicon. Categories may include Lithologic Characteristics, Thickness and Distribution, Relationships to Other Units, History etc.','USER','dbo','TABLE','RA_STRAT_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC EQUIVALENCE DIRECTIONALITY: Use this table to record all possible names, codes and other identifiers for the direction in which the equivalence is valid, such as one way or two way. A lithstratigraphic bed may be equivalentto a formation, but the formation is not necessarily equivalent to the bed.','USER','dbo','TABLE','RA_STRAT_EQUIV_DIRECT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC EQUIVALENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of relationship between two units, two surfaces, or a unit and surface based on an interpretation that the two strat elements are the same age (equivalent stratigraphically and/or geochronologically), although they are seperated in space.','USER','dbo','TABLE','RA_STRAT_EQUIV_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC FIELD STATION NODE LOCATION TYPE: Use this table to record all possible names, codes and other identifiers for the type of location for a field station node. Locations may exist across the surface of a field station or vertically, to describe depths. Types may include sample point, corner, hole surface, hole bottom etc.','USER','dbo','TABLE','RA_STRAT_FLD_NODE_LOC';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC HIERARCHY TYPE: Use this table to record all possible names, codes and other identifiers for the type of hierarchy that is defined, such as biostratigraphic (Era, Period, Series...) lithostratigraphic etc.','USER','dbo','TABLE','RA_STRAT_HIERARCHY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC INTERPRETATION METHOD: Use this table to record all possible names, codes and other identifiers for the Interpretation Method - surface sample, sub-surface, logs, seismic, etc.','USER','dbo','TABLE','RA_STRAT_INTERP_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC NAME SET TYPE: Use this table to record all possible names, codes and other identifiers for the type of name set, such as vendor, scientific, working, corporate, lexicon etc.','USER','dbo','TABLE','RA_STRAT_NAME_SET_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC OCCURRENCE TYPE: Use this table to record all possible names, codes and other identifiers for how a strat unit occurs relative to another strat unit. Examples may be contained, interfinger, etc.','USER','dbo','TABLE','RA_STRAT_OCCURRENCE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC STATUS TYPE: Use this table to record all possible names, codes and other identifiers for the status of the stratigraphic unit, such as whether the strat unit is currently in use, obsolete, replaced etc.','USER','dbo','TABLE','RA_STRAT_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC TOPOLOGICAL RELATIONSHIP TYPE: Use this table to record all possible names, codes and other identifiers for the type of topological relationship between two stratigraphic units, such as bounded by, adjacent to, interfingered within, overlies, underlies etc.','USER','dbo','TABLE','RA_STRAT_TOPO_RELATION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHY TYPE: Use this table to record all possible names, codes and other identifiers for the category of stratigraphy that the STRAT UNIT is described within, such as lithostratigraphic, chronostratigraphic, biostratigraphic, radiometric etc.','USER','dbo','TABLE','RA_STRAT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC UNIT COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a stratigraphical unit','USER','dbo','TABLE','RA_STRAT_UNIT_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC UNIT DESCRIPTION: Use this table to record all possible names, codes and other identifiers for any descriptive charateristic of the stratigraphic unit, such as the color or texture that may be used to identify the stratigraphic unit. Likely to be replaced in future work.','USER','dbo','TABLE','RA_STRAT_UNIT_DESC';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRAT UNIT QUALIFIER: Use this table to record all possible names, codes and other identifiers for a qualifier that describes where on a stratigraphic unit an event is described, such as for a land right description, which may be grantedto the TOP or BASE of a stratigraphic unit.','USER','dbo','TABLE','RA_STRAT_UNIT_QUALIFIER';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STRATIGRAPHIC UNIT TYPE: Use this table to record all possible names, codes and other identifiers for the type of stratigraphic unit, often described in terms of its position in a hierarchical scale, such as Eon, Epoch, Bed, Formation etc.','USER','dbo','TABLE','RA_STRAT_UNIT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STREAMER COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of streamer component, such as hydrophone, depth reading, stretch section etc.','USER','dbo','TABLE','RA_STREAMER_COMP';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STREAMER POSITION: Use this table to record all possible names, codes and other identifiers for the position of the streamer in the array, such as surface, sea floor etc.','USER','dbo','TABLE','RA_STREAMER_POSITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS STUDY TYPE: Use this table to record all possible names, codes and other identifiers for the type of study that is described, such as organic geochemistry, maceral analysis, paleontological analysis, benchmark environmental study, etc','USER','dbo','TABLE','RA_STUDY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SUBSTANCE COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of relationship between the substance and its composite parts. For example, a sub-substance might be a chemical fraction of the parent, or a product that is created duing processing','USER','dbo','TABLE','RA_SUBSTANCE_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SUBSTANCE PROPERTY TYPE: Use this table to record all possible names, codes and other identifiers for valid properties that may be defined in SUBSTANCE PROPERTY DETAIL. This table is parent to the control column, which is used to control the behavior of properties that are entered into SUBSTANCE PROPERTY TYPE. Care should be taken in populating and managing this table, taking into account the VERTICAL TABLE procedure recommendations in the PPDM WIKI.','USER','dbo','TABLE','RA_SUBSTANCE_PROPERTY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SUBSTANCE USE RULE: Use this table to record all possible names, codes and other identifiers for a rule that describes in more detail a rule controlling when or how this substance definition should be used.','USER','dbo','TABLE','RA_SUBSTANCE_USE_RULE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SUBSTANCE CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of relationship between two substances, other than compositional relationships.','USER','dbo','TABLE','RA_SUBSTANCE_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SOFTWARE BUSINESS ASSOCIATE ROLE: Use this table to record all possible names, codes and other identifiers for the role that a business associate has in a software application. May include the vendor who created it, the vendor support resources, internal support resources etc.','USER','dbo','TABLE','RA_SW_APP_BA_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SOFTWARE APPLICATION FUNCTION: Use this table to record all possible names, codes and other identifiers for valid functions that a software application may have. Includes word processing, calculations, geologic interpretation, accounting, production accounting etc.','USER','dbo','TABLE','RA_SW_APP_FUNCTION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SOFTWARE APPLICATION TYPE:  Use this table to hold alternate names for the type of function that an application used.  Effective if applications should be grouped into collections (such as all back ofice applications, or all drilling applications)','USER','dbo','TABLE','RA_SW_APP_FUNCTION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SOFTWARE APPLICATION CROSS REFERENCE: Use this table to record all possible names, codes and other identifiers for the reason why you have to cross referenced applications to each other. This is useful to keep track of software products thatreplace others, or products that provide a data input to another application, or accept an input from another. You can also use it to indicate dependencies in workflows (which application is used before, after or in conjunction with anothe','USER','dbo','TABLE','RA_SW_APP_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TAX CREDIT CODE: Use this table to record all possible names, codes and other identifiers for a Code indicating the well qualifies for a tax credit. "C" = credit for the well being permitted for coalbed methane gas.','USER','dbo','TABLE','RA_TAX_CREDIT_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TEST EQUIPMENT: Use this table to record all possible names, codes and other identifiers for specific types of equipment used for well testing. For example casing packer, hook wall packer or straddle packer.','USER','dbo','TABLE','RA_TEST_EQUIPMENT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TEST PERIOD TYPE: Use this table to record all possible names, codes and other identifiers for a well test time pressure period type. For example hydrostatic, shutin, valve open or flowing.','USER','dbo','TABLE','RA_TEST_PERIOD_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TEST RECOVERY METHOD: Use this table to record all possible names, codes and other identifiers for the method by which fluid was recovered for a well test. For example pipe or chamber.','USER','dbo','TABLE','RA_TEST_RECOV_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TEST RESULT: Use this table to record all possible names, codes and other identifiers for the final result of a well test. For example successful, misrun, pipe failure, packer failure or tester plugged.','USER','dbo','TABLE','RA_TEST_RESULT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SHUTOFF TYPE: Use this table to record all possible names, codes and other identifiers for a Code identifying the type of shutoff used in the wellbore (e.g., bridge plug, cased off, plugged off, or squeezed etc.).','USER','dbo','TABLE','RA_TEST_SHUTOFF_TYPE';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_TEST_SUBTYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TIMEZONE: Use this table to record all possible names, codes and other identifiers for a valid list of time zones. You can obtain a list of timezones from www.worldtimezone.com.','USER','dbo','TABLE','RA_TIMEZONE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TITLE OWNERSHIP TYPE: Use this table to record all possible names, codes and other identifiers used to refer to type of ownership for titles only. May be life estate holder, joint tenant, tentan in common...','USER','dbo','TABLE','RA_TITLE_OWN_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TOUR OCCURENCE TYPE: Use this table to record all possible names, codes and other identifiers for a type of well activity that can be described as a well tour occurence. For example blowout or lost circulation.','USER','dbo','TABLE','RA_TOUR_OCCURRENCE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS HEADER FORMAT: Use this table to record all possible names, codes and other identifiers for the header format used by the trace data, such as IEEE float, IBM float, 32 bit, 16 bit etc.','USER','dbo','TABLE','RA_TRACE_HEADER_FORMAT';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TRACE HEADER WORD: Use this table to record all possible names, codes and other identifiers for allowed trace header words for seismc trace data.','USER','dbo','TABLE','RA_TRACE_HEADER_WORD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TRANSACTION COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component that is associated with this transaction. Could be a seismic set that was sold, the jurisdiction in which the transaction occured, the business associate who purchased the data, a broker who negotiated the transaction, the contract that governed the transaction etc. Could include sent to client, billing, used for inspection, project control etc.','USER','dbo','TABLE','RA_TRANS_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TRANSACTION STATUS: Use this table to record all possible names, codes and other identifiers for the status of this transaction, such as complete and paid, pending approval, approved, cancelled etc.','USER','dbo','TABLE','RA_TRANS_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS SEISMIC TRANSACTION TYPE: Use this table to record all possible names, codes and other identifiers for the type of seismc transaction that has been arranged, such as sale, trade, lease etc.','USER','dbo','TABLE','RA_TRANS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TREATMENT FLUID: Use this table to record all possible names, codes and other identifiers for the type of treating fluid used in the treatment operation of the well. For example, Oil, Water, Acid.','USER','dbo','TABLE','RA_TREATMENT_FLUID';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL TREATMENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of treatment job performed on the well. For example, hydraulic fracturing, acidizing, nitroglycerine explosives etc.','USER','dbo','TABLE','RA_TREATMENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TUBING GRADE: Use this table to record all possible names, codes and other identifiers for the tensile strength of the tubing material. A system of classifying the material specifications for steel alloys used in the manufacture of tubing.','USER','dbo','TABLE','RA_TUBING_GRADE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TUBING TYPE: Use this table to record all possible names, codes and other identifiers for the particular type of tubular or type. For example tubing, casing or liner. This is a general classification. A more specific description can be found in R_LINER_TYPE.','USER','dbo','TABLE','RA_TUBING_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TVD METHOD: Use this table to record all possible names, codes and other identifiers for the method used to determine the true vertical depth.','USER','dbo','TABLE','RA_TVD_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS VALUE QUALITY: Use this table to record all possible names, codes and other identifiers for the quality of data in a row, usually with reference to the method or procedures used to load the data, although other types of quality reference are permitted.','USER','dbo','TABLE','RA_VALUE_QUALITY';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS VELOCITY COMPUTATION METHOD: Use this table to record all possible names, codes and other identifiers for methods for computing seismic velocity. For example, checkshot survey, interpolation...','USER','dbo','TABLE','RA_VELOCITY_COMPUTE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS VELOCITY DIMENSION: Use this table to record all possible names, codes and other identifiers for the dimensions of the velocites that have been collected, usually point or interval velocities.','USER','dbo','TABLE','RA_VELOCITY_DIMENSION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS VELOCITY TYPE: Use this table to record all possible names, codes and other identifiers for the valid types of seismic velocity. Horizontal velocities and vertical velocites are typical types.','USER','dbo','TABLE','RA_VELOCITY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS VERTICAL DATUM TYPE: Use this table to record all possible names, codes and other identifiers for valid types of Vertical Datums. For example, geoidal height the height above the geoid, elevation the height above mean sea level.','USER','dbo','TABLE','RA_VERTICAL_DATUM_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS VESSEL REFERENCE POINT: Use this table to record all possible names, codes and other identifiers for the point to which the offsets are referenced. In many cases, this is the primary antenna, but in some cases other positions on the vessel are used as the reference point.','USER','dbo','TABLE','RA_VESSEL_REFERENCE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS VESSEL SIZE: Use this table to record all possible names, codes and other identifiers for the vessel size.','USER','dbo','TABLE','RA_VESSEL_SIZE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS VOLUME FRACTION: Use this table to record all possible names, codes and other identifiers for the type of oil that was separated via the fractional distillation.','USER','dbo','TABLE','RA_VOLUME_FRACTION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS VOLUME METHOD: Use this table to record all possible names, codes and other identifiers for the type of method used to determine the volume of fluids moved. Examples would be measured, prorated, engineering study, etc.','USER','dbo','TABLE','RA_VOLUME_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS VERTICAL SEISMIC PROFILE TYPE: Use this table to record all possible names, codes and other identifiers for valid types of VSP. For example, upgoing, downgoing, ...','USER','dbo','TABLE','RA_VSP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WASTE ADJUSTMENT REASON: Use this table to record all possible names, codes and other identifiers for the reason for an adjustment between the shipped and received volume, such as temperature based shrinkage or expansion, spillage, evaporation etc.','USER','dbo','TABLE','RA_WASTE_ADJUST_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WASTE FACILITY TYPE: Use this table to record all possible names, codes and other identifiers for the type of waste handling facility, such as a pit, incinerator etc.','USER','dbo','TABLE','RA_WASTE_FACILITY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERNECE ALIAS WASTE HANDLING METHOD: Use this table to record all possible names, codes and other identifiers for the method used to handle disposal of the waste material, such as incineration, neutralization, burying etc.','USER','dbo','TABLE','RA_WASTE_HANDLING';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WASTE HAZARD: Use this table to record all possible names, codes and other identifiers for the hazardous nature of the material, such as dangerous goods.','USER','dbo','TABLE','RA_WASTE_HAZARD_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WASTE MATERIAL: Use this table to record all possible names, codes and other identifiers for the material that has been shipped as waste, such as drill mud.','USER','dbo','TABLE','RA_WASTE_MATERIAL';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WASTE ORIGIN TYPE: Use this table to record all possible names, codes and other identifiers for the type of location that this waste originated from, such as facility or well.','USER','dbo','TABLE','RA_WASTE_ORIGIN';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WATER BOTTOM ZONE: Use this table to record all possible names, codes and other identifiers for valid water bottom zones. This code is retained in Louisiana as special allowable area or zone.','USER','dbo','TABLE','RA_WATER_BOTTOM_ZONE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WATER CONDITION TYPE: Use this table to record all possible names, codes and other identifiers for valid conditions of a large water body, such as an ocean, sea, gulf or lake. Could include values such as choppy, high swell, rough.','USER','dbo','TABLE','RA_WATER_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WATER DATUM: Use this table to record all possible names, codes and other identifiers for reference datum to which the water depth is referenced, such as mean sea level.','USER','dbo','TABLE','RA_WATER_DATUM';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WATER PROPERTY CODE: Use this table to record all possible names, codes and other identifiers for the Water Property Code.','USER','dbo','TABLE','RA_WATER_PROPERTY_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WEATHER CONDITION TYPE: Use this table to record all possible names, codes and other identifiers for valid kinds of weather conditions such as sunny and calm, rain showers, snow, ice fog etc.','USER','dbo','TABLE','RA_WEATHER_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS CAUSE TYPE: Use this table to record all possible names, codes and other identifiers for the type of cause that resulted in a change to the activity in the well. Often, things that cause downtimes (cessation of production, constrained production or deferred production). Causes are usually defined hierarchically.','USER','dbo','TABLE','RA_WELL_ACTIVITY_CAUSE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL ACTIVITY COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a well activity.','USER','dbo','TABLE','RA_WELL_ACTIVITY_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL ACTIVITY TYPE EQUIVALENCE: Use this table to record all possible names, codes and other identifiers for the equivalence measure, so that you can set up equivalences for activities that are closely related, or for activities that subsume or are fully equivalent with another activity type.','USER','dbo','TABLE','RA_WELL_ACT_TYPE_EQUIV';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL ALIAS LOCATION TYPE: Use this table to record all possible names, codes and other identifiers for the position on the wellbore that this alias is assigned to. Common types are top hole and bottom hole.','USER','dbo','TABLE','RA_WELL_ALIAS_LOCATION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL PRESSURE CIRCULATION TYPE: Use this table to record all possible names, codes and other identifiers for values that indicate whether one or both pumps were on the hole, such as Single, Combined, Parallel.','USER','dbo','TABLE','RA_WELL_CIRC_PRESS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL CLASS: Use this table to record all possible names, codes and other identifiers for the classification of a well. This may include, but is not restricted to the Lahee classification scheme. For example development, new field wildcat or outpost.','USER','dbo','TABLE','RA_WELL_CLASS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component associated with a well.','USER','dbo','TABLE','RA_WELL_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL DATUM TYPE: Use this table to record all possible names, codes and other identifiers for the type of point or horizontal surface used as an elevation reference for measurements in a well. Examples: kelly bushing, ground, sea level.','USER','dbo','TABLE','RA_WELL_DATUM_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL DOWNTIME TYPE: Use this table to record all possible names, codes and other identifiers for the type of downtime experienced during a well operation or event. Downtime types may include downtime, constrained production, deferred production. Added to allow some granularity of describing downtime events wthout having to overload the event type table.','USER','dbo','TABLE','RA_WELL_DOWNTIME_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL DRILLING OPERATIONS TYPE: Use this table to record all possible names, codes and other identifiers for the type of drilling operation that the bit is doing, such as drilling, coring or reaming.','USER','dbo','TABLE','RA_WELL_DRILL_OP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL FACILITY USE TYPE: Use this table to record all possible names, codes and other identifiers for the type of use that a facility is put to for a particular well, such as processing, pumping station, steam production etc.','USER','dbo','TABLE','RA_WELL_FACILITY_USE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL LEVEL TYPE: Use this table to record all possible names, codes and other identifiers for which well component this row describes, as outlined in www.WhatIsAWell.org. Values may include WELL, WELL ORIGIN, WELLBORE, WELLBORE SEGMENT, WELLBORE COMPLETION or WELLBORE CONTACT INTERVAL.','USER','dbo','TABLE','RA_WELL_LEVEL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL LICENSE CONDITION TYPE: Use this table to record all possible names, codes and other identifiers for the type of condition applied to the well license, such as flaring rate, venting rate, production rate, H2S content limit, emissions etc.','USER','dbo','TABLE','RA_WELL_LIC_COND';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_WELL_LIC_COND_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL LICENSE DUE CONDITION: Use this table to record all possible names, codes and other identifiers for the state that must be achieved for the condition to become effective. For example, a report may be due 60 days after operations commence (or cease).','USER','dbo','TABLE','RA_WELL_LIC_DUE_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL LICENSE VIOLATION RESOLUTION TYPE: Use this table to record all possible names, codes and other identifiers for the type of resolution to a violation of a license term, such as the payment of a fine or creation of new processes.','USER','dbo','TABLE','RA_WELL_LIC_VIOL_RESOL';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL LICENSE VIOLATION TYPE: Use this table to record all possible names, codes and other identifiers for the type of violation of a license that is being recorded. Can be as simple as failure to submit necessary reports or something moredifficult such as improper procedures.','USER','dbo','TABLE','RA_WELL_LIC_VIOL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL LOG CLASS: Use this table to record all possible names, codes and other identifiers for the classes of well log files that may be created.','USER','dbo','TABLE','RA_WELL_LOG_CLASS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL NODE STRAT UNIT PICK METHOD: Use this table to record all possible names, codes and other identifiers for the method used to pick the distance from a well node to the top and base of a stratigraphic unit. Could be logs, cores, other kinds of studies etc.','USER','dbo','TABLE','RA_WELL_NODE_PICK_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELLBORE PROFILE SHAPE: Use this table to record all possible names, codes and other identifiers for a type of wellbore shape. For example vertical, horizontal, directional or s-shaped.','USER','dbo','TABLE','RA_WELL_PROFILE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL QUALIFICATION TYPE: Use this table to record all possible names, codes and other identifiers for the type of method used to determine that the well is capable of producing in paying quantities. test, logs,','USER','dbo','TABLE','RA_WELL_QUALIFIC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL REFERENCE VALUE TYPE: Use this table to record all possible names, codes and other identifiers for the case where a measured value must be referenced to another value (such as time, depth or an instrument setting), capture the type ofreference value here.','USER','dbo','TABLE','RA_WELL_REF_VALUE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL RELATIONSHIP: Use this table to record all possible names, codes and other identifiers for the type of relationship a well/wellbore may have with a parent well/wellbore. For example sidetracked, recompleted or deepening.','USER','dbo','TABLE','RA_WELL_RELATIONSHIP';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL SERVICE METRIC: Use this table to record all possible names, codes and other identifiers for the types of metrics that are captured for each service provided on a well, as reported by reporting period (shift, day, tour etc.). Metrics such as hours, volumes pumped, distance completed etc can be captured. Actual costs should be reported using the FINANCE module.','USER','dbo','TABLE','RA_WELL_SERVICE_METRIC';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL SERVICE METRIC CODE: Use this table to record all possible names, codes and other identifiers for the value for a metric, such as started, finished or quality of service. Codes are created with reference to the type of metric that is being captured.','USER','dbo','TABLE','RA_WELL_SERV_METRIC_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL SET ROLE: Use this table to record all possible names, codes and other identifiers for the role that this well record supports in the wellset. This may include values such as service, relief, original skidded, abandoned, planned, not drilled etc.','USER','dbo','TABLE','RA_WELL_SET_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL SET TYPE: Use this table to record all possible names, codes and other identifiers for the kind of well set that is described. Mainly used to group all of the components in a well through the life cycle, so that all the well objects from planning to disposal (whether or not successful or even physically created) can be gathered together.','USER','dbo','TABLE','RA_WELL_SET_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL SUPPORT FACILITY USE TYPE: Use this table to record all possible names, codes and other identifiers for the type of use that a support facility is put to for a particular well, such as access, drilling, communications etc.','USER','dbo','TABLE','RA_WELL_SF_USE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL STATUS: Use this table to record all possible names, codes and other identifiers for the status of the well.','USER','dbo','TABLE','RA_WELL_STATUS';
go

execute sp_addextendedproperty 'Description','Use this table to record all possible names, codes and other identifiers for','USER','dbo','TABLE','RA_WELL_STATUS_QUAL';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL STATUS QUALIFIER VALUE: Use this table to record all possible names, codes and other identifiers for the valid values for each of the well status qualifiers defined by the well status workgroup http://www.ppdm.org/standards/wellstatus. This table should only be used to store the values for qualifiers, and not for well status values themselves. For example, in the well status facet FLUID TYPE, a valid qualifier is ABUNDENCE (this value goes in R_WELL_STATUS_QUAL). This table will capture the values PRIMARY, SECONDARY, SHOW and TRACE.','USER','dbo','TABLE','RA_WELL_STATUS_QUAL_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL STATUS PLOT SYMBOL:  Use this table to record all possible names, codes and other identifiers to associate each plot symbol with the appropriate facet definition or definitions that contribute to its construction.  As described in Well Status http://www.ppdm.org/standards/wellstatus, the plot symbols may be based on one or more facet definitions that are used IN COMBINATION to create a symbol.  The column FACET_COUNT is used to state how many facets are used in the creation of each plot symbol.','USER','dbo','TABLE','RA_WELL_STATUS_SYMBOL';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL STATUS TYPE: Use this table to record all possible names, codes and other identifiers for the type of status reported for the well. These should include the facet values from Well Status http://www.ppdm.org/standards/wellstatus. Each facet type is one row in this table. As needed, statuses that are reported from various agencies may also be included.','USER','dbo','TABLE','RA_WELL_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL STATUS CROSS-REFERENCE(PLOT SYMBOL): Use this table to record all possible names, codes and other identifiers used to cross-reference Well Statuses. As described in Well Status http://www.ppdm.org/standards/wellstatus, the plot symbolsmay be based on one or more facet definitions that are used IN COMBINATION to create a symbol.','USER','dbo','TABLE','RA_WELL_STATUS_XREF';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL TEST TYPE: Use this table to record all possible names, codes and other identifiers for the general type of test used to evaluate the potential of the well. For example, Drill Stem Tests (DST), Repeat Formation Tests (RFT), Initial Potential(IP).','USER','dbo','TABLE','RA_WELL_TEST_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of cross reference between two wells. This may include relationships between well and well bore (also handled more simply by PARENT UWI) or planned and actual wells. Functional relationships may also be captured if desired.','USER','dbo','TABLE','RA_WELL_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WELL ZONE INTERVAL VALUE TYPE: Use this table to record all possible names, codes and other identifiers for the type of value that is associated with this well zone interval. Note that where possible, it is preferable to use explicit tables - by choice, if possible, this table should only be used when no other table can be found.','USER','dbo','TABLE','RA_WELL_ZONE_INT_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WIND STRENGTH: Use this table to record all possible names, codes and other identifiers for the strength of the wind, often measured according to a standard list of wind strengths, such as the Beaufort Wind Scale (www.bom.gov.au/lam/glossary/beaufort.shtml)','USER','dbo','TABLE','RA_WIND_STRENGTH';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WORK BID TYPE: Use this table to record all possible names, codes and other identifiers for the type of bid component that is part of a work bid. Examples may include drilling, shooting seismic etc.','USER','dbo','TABLE','RA_WORK_BID_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WORK ORDER BUSINESS ASSOCIATE ROLE: Use this table to record all possible names, codes and other identifiers for valid roles played by business associates with respect to carrying out instructions for a work order. Roles include shipping address,client etc.','USER','dbo','TABLE','RA_WO_BA_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WORK ORDER COMPONENT TYPE: Use this table to record all possible names, codes and other identifiers for the type of component that has been associated with the work order, such as governing contract, seismic associated, projects associated etc. Additional FKs may be added to this table. Associations with obligations (related to payments etc) are captured in OBLIGATION COMPONENT.','USER','dbo','TABLE','RA_WO_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WORK ORDER CONDITION: Use this table to record all possible names, codes and other identifiers for the pre-conditions that must exist in order for this work order to be completed or filled. Can indicate a need for approval, payment, for a facility to be taken off-line (for maintenance), for notification to be sent out etc.','USER','dbo','TABLE','RA_WO_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WORK ORDER DELIVERY TYPE: Use this table to record all possible names, codes and other identifiers for the type of delivery, such as received product, sent product, returned products etc.','USER','dbo','TABLE','RA_WO_DELIVERY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WORK ORDER INSTRUCTION: Use this table to record all possible names, codes and other identifiers for coded values for specific instructions associated with work orders. May be used in conjunction with description fields as appropriate.','USER','dbo','TABLE','RA_WO_INSTRUCTION';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WORK ORDER TYPE: Use this table to record all possible names, codes and other identifiers for the type of work order, such as data circulation, flowing hole remediation, brokerage etc.','USER','dbo','TABLE','RA_WO_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS WORK ORDER CROSS REFERENCE TYPE: Use this table to record all possible names, codes and other identifiers for the type of relationships between work orders. These relationships may be historical (work order replaces another), functional (this work order was divided into two subordinate work orders) or transactional (company A sent a work order which was used to create a new work order).','USER','dbo','TABLE','RA_WO_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REPORTING HIERARCHY: This table tracks the available hierarchies which may be used for reporting purposes.','USER','dbo','TABLE','REPORT_HIER';
go

execute sp_addextendedproperty 'Description','REPORTING HIERARCHY ALIAS: Tracks all the names, codes and identifiers associated with a reporting hierarchy.','USER','dbo','TABLE','REPORT_HIER_ALIAS';
go

execute sp_addextendedproperty 'Description','REPORTING HIERARCHY TEMPLATE DESCRIPTION: This table defines specific levels of a hierarchy template. For example, a hierarchy may use countries, provinces and fields. This table does not describe a specific instance of a hierarchy but will be used as a template for creating new valid hierarchies.','USER','dbo','TABLE','REPORT_HIER_DESC';
go

execute sp_addextendedproperty 'Description','REPORTING HIERARCHY LEVEL: This table places reserves entities or production entites at a specific level of a hierarchy. Note that more than one object may exist at any given level.','USER','dbo','TABLE','REPORT_HIER_LEVEL';
go

execute sp_addextendedproperty 'Description','RESERVE HIERARCHY TEMPLATE TYPE: this table serves as the header for valid reporting hiearchy templates. A template will be used to indicate that this hierarchy goes COUNTRY - STATE - FIELD. Another hierarchy may be defined as CONTINENT - COUNTRY - FIELD - POOL. Actual implementations of the hierarchies are stored in RESENT REPORT HIER.','USER','dbo','TABLE','REPORT_HIER_TYPE';
go

execute sp_addextendedproperty 'Description','REPORTING HIERARCHY USE: this table tracks which production entities (PDEN) or reserves entities (RESENT) are associated with a hierarchy. Entities may be associated at the lowest level of the hierarchy and then rolled up via the recursive relationship in RESENT HIER LEVEL, or stored explicitly in this table. For convenience, this table remains part of the reserves module, but users should be aware that it can be shared.','USER','dbo','TABLE','REPORT_HIER_USE';
go

execute sp_addextendedproperty 'Description','RESERVE ENTITY CLASS: This table identifies the reserve classes which have been created for a given reserve entity.','USER','dbo','TABLE','RESENT_CLASS';
go

execute sp_addextendedproperty 'Description','RESERVE ENTITY COMPONENT: This table identifies the product components which have been created for a given reserve entity.','USER','dbo','TABLE','RESENT_COMPONENT';
go

execute sp_addextendedproperty 'Description','RESERVE ENTITY ECONOMIC RUN: This table identifies the summary value indicators associated with the economic evaluation of a specific reserve entity, reserve class, and economic scenario. It is the header table for a detailed economics run.','USER','dbo','TABLE','RESENT_ECO_RUN';
go

execute sp_addextendedproperty 'Description','RESERVE ENTITY ECONOMIC SCHEDULE: This table is used to outline the economic value of the reserve withing certain economic parameters.','USER','dbo','TABLE','RESENT_ECO_SCHEDULE';
go

execute sp_addextendedproperty 'Description','RESERVE ENTITY ECONOMIC VOLUME: This table is used to capture summary information about remaining balances for various products in the economics scenario.','USER','dbo','TABLE','RESENT_ECO_VOLUME';
go

execute sp_addextendedproperty 'Description','RESERVES ENTITY PRODUCT: This table tracks the products for which reserves are calculated or tracked.','USER','dbo','TABLE','RESENT_PRODUCT';
go

execute sp_addextendedproperty 'Description','RESERVES ENTITY PRODUCT PROPERTY: This table tracks the product properties for specific reserve entities.','USER','dbo','TABLE','RESENT_PROD_PROPERTY';
go

execute sp_addextendedproperty 'Description','RESERVE REVISION CATEGORY: The allowable revision categories which may be used to assign a change in reserve volumes to a specific type of business activity, such as drilling and exploration well.','USER','dbo','TABLE','RESENT_REVISION_CAT';
go

execute sp_addextendedproperty 'Description','RESERVES ENTITY VOLUME UNIT REGIME: This table keeps track of which unit regime should be used for each reserve entity through the life cycle of that reserves entity.','USER','dbo','TABLE','RESENT_VOL_REGIME';
go

execute sp_addextendedproperty 'Description','RESERVE ENTITY VOLUME REVISION: This table tracks the approvals and revision categories assosiated with any changes made to the reserve volumes (RESENT_VOL_SUMMARY table).','USER','dbo','TABLE','RESENT_VOL_REVISION';
go

execute sp_addextendedproperty 'Description','RESERVE ENTITY VOLUME SUMMARY: This table tracks the remaining reserves (both gross and net) for each reserve entity, for a specific time period. This table also identifies the method used to establish these volumes, and the entry(s) in the decline analysis, material balance and volumetric analysis table associated with these volumes. Changes in these volumes from one time period to the next are tracked in the RESENT_VOL_REVISION table.','USER','dbo','TABLE','RESENT_VOL_SUMMARY';
go

execute sp_addextendedproperty 'Description','RESERVE ENTITY CROSS REFERENCE: This table allows the merging or segragation of reserve entities to be tracked.','USER','dbo','TABLE','RESENT_XREF';
go

execute sp_addextendedproperty 'Description','RESERVE CLASSIFICATION: This table identifies the reserve classifications which have been set up to define the status (developed, producing) and confidence (proved, probable) of established reserve volumes.','USER','dbo','TABLE','RESERVE_CLASS';
go

execute sp_addextendedproperty 'Description','RESERVE CLASS CALCULATION: This table identifies methods for calculating classes, depending on what classes you have reserves calculated already. Formula details for each method are stored in RESERVE CLASS FORMUAL.','USER','dbo','TABLE','RESERVE_CLASS_CALC';
go

execute sp_addextendedproperty 'Description','RESERVE CLASS FORMULA: This table identifies the mathmatical relation ship between the reserve classes which have been set up. TOTAL PROVED = PROVED DEVELOPED + PROVED UNDEVELOPED','USER','dbo','TABLE','RESERVE_CLASS_FORMULA';
go

execute sp_addextendedproperty 'Description','RESERVE ENTITY: A reserve entity is a grouping of production objects (real or theoretical) that are grouped for the purposes of analyzing and forecasting production reserves. A reserve entity may be as small as a single well or a large as desired.','USER','dbo','TABLE','RESERVE_ENTITY';
go

execute sp_addextendedproperty 'Description','RESTRICTION: Describes surface restrictions of various sorts, as defined and enforced by a jurisdictional body, such as a government or its agency. Detailed information about the surface restriction, such as its areal extent, restricted activities, contact information and descriptions can be found in associated tables.','USER','dbo','TABLE','RESTRICTION';
go

execute sp_addextendedproperty 'Description','RESTRICTION ACTIVITY: The activities that are restricted for this restriction. Some restrictions are permanent, and others are seasonal.','USER','dbo','TABLE','REST_ACTIVITY';
go

execute sp_addextendedproperty 'Description','RESTRICTION CLASSIFICATION: the code or type of the restriction classification. For example, Bird Sanctuaries in Alberta have a Class code BSA.','USER','dbo','TABLE','REST_CLASS';
go

execute sp_addextendedproperty 'Description','RESTRICTION CONTACT: This table provides a list of valid contacts for the restriction. Contact information may be versioned over time, and the current primary contact flagged.','USER','dbo','TABLE','REST_CONTACT';
go

execute sp_addextendedproperty 'Description','RESTRICTION REMARK: General remarks about the restriction.','USER','dbo','TABLE','REST_REMARK';
go

execute sp_addextendedproperty 'Description','RESTRICTION TEXT: the text of the restriction document, as provided by the regulatory agency or board. The SEQ_NO is used to order rows for retrieval.','USER','dbo','TABLE','REST_TEXT';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT SEISMIC TRACE AUXILLARY CHANNEL: The table is used to list the auxillary channels and their use in the field record.','USER','dbo','TABLE','RM_AUX_CHANNEL';
go

execute sp_addextendedproperty 'Description','RECORD OR PRODUCT CIRCULATION: Describes the Records Management function of checking items in and out, due dates and authorizations.','USER','dbo','TABLE','RM_CIRCULATION';
go

execute sp_addextendedproperty 'Description','DATA CIRCULATION PROCESS: lists the processes conducted during circulation. for example, a tape copy may be made and sent to another site. This would not create another physical item (as it is not owned), but the record of its creation at that time is needed, together with indication when it was forwarded to the site.','USER','dbo','TABLE','RM_CIRC_PROCESS';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT COMPOSITE ITEM: An item that has been entered into records management that is a composite of several items, such as a portfolio of products, a poster containing several objects (maps, diagrams, text etc.). This is a valid sub type of RM INFORMATION ITEM. In this table, the value of INFO_ITEM_TYPE may only be the name of this table.','USER','dbo','TABLE','RM_COMPOSITE';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT COPY RECORD: this table can be used to capture the relationships between records in digital media as the media is copied. Generally used for tape copies, to allow capture of logical record relationships.','USER','dbo','TABLE','RM_COPY_RECORD';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT CREATOR: allows multiple authorships to be captured for documents, reports, maps or other products being managed in the module.','USER','dbo','TABLE','RM_CREATOR';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT CUSTODY: this table can be used to track the chain of custody for a document or other product. Usually this is required to demonstrate the provenance of an object in the event it may need to be entered as evidence in court.','USER','dbo','TABLE','RM_CUSTODY';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT DATA CONTENT: Can be used to describe the intersection between physical item and information item. Keyed by Physical item, information item and a sequencenumber. Can be used to describe the contents of a file folder, to show which lines are on which tape, and which tape contains which lines etc .','USER','dbo','TABLE','RM_DATA_CONTENT';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT DATA STORE: Any location where data is stored. Can be a physical location (such as a building, bay or slot) or a digital location (such as a file server or path name).','USER','dbo','TABLE','RM_DATA_STORE';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT DATA STORE HIERARCHY: This table and its child RM DATA STORE HIER LEVEL, store the theoretical hierarchy description that exists in a data storage facility. Use this pair of tables to ensure that each data store that is assigned is located correctly in a hierarchy. For example, if a data store contains bays, which contain shelving units care should be taken to ensure that this hierarchy is properly used each time a new data store is created.','USER','dbo','TABLE','RM_DATA_STORE_HIER';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT DATA STORE HIERARCHY LEVEL: This table stores the theoretical hierarchy description that exists in a data storage facilty. Use this pair of tables to ensure that each data store that is assigned is located correctly in a hiearchy. For example, if a data store contains bays, which contain shelving units care should be taken to ensure that this hierarchy is properly used each time a new data store is created.','USER','dbo','TABLE','RM_DATA_STORE_HIER_LEVEL';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT DATA STORAGE SYSTEM ITEM: Use this table to indicate the kinds of items that a data store strucutre is supposed to store. For example, some systems may be designed for seismic trace data and others for well files or core samples.','USER','dbo','TABLE','RM_DATA_STORE_ITEM';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT DATA STORAGE SYSTEM MEDIA: the types of media that this data store is authorized or designed to manage. Can be used for site management.','USER','dbo','TABLE','RM_DATA_STORE_MEDIA';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT DATA STORAGE SYSTEM STRUCTURE: Use this table to describe the physical structure of your data stores. For example, you can describe the dimensions of the data store. You may use this table to define groups of data stores that have similar or related functions.','USER','dbo','TABLE','RM_DATA_STORE_STRUCTURE';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT DECRYPTION KEY: The decryption key that is to be used for the encoded data file.','USER','dbo','TABLE','RM_DECRYPT_KEY';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT DOCUMENT: An item that has been entered into records management that is a document, such as a contract, land agreement, report etc. This is a valid sub type of RM INFORMATION ITEM. In this table, the value of INFO_ITEM_TYPE may only be the nameof this table.','USER','dbo','TABLE','RM_DOCUMENT';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT PHYSICAL ITEM ENCODING: this table is used to capture the multiple levels of encoding that a file may be subjected to prior to storage and cataloguing. For example, a file may be in RODE format, then zipped. In this case, you must know how the file was encoded, the order in which the encoding steps were performed and the method to be used to un encode the data.','USER','dbo','TABLE','RM_ENCODING';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT EQUIPMENT: An item that has been entered into records management that is a piece of equipment, such as an instrument or laptop. These items may be tracked using the Records module. This is a valid sub type of RM INFORMATION ITEM. In this table, the value of INFO_ITEM_TYPE may only be the name of this table.','USER','dbo','TABLE','RM_EQUIPMENT';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT FILE CONTENT: This table may be used to capture the digital contents of a file if desired.','USER','dbo','TABLE','RM_FILE_CONTENT';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT FOSSIL: A fossil that has been placed in the records managment archive. The fossil may be the actual fossil or a representation of the fossil. This is a valid sub type of RM INFORMATION ITEM. In this table, the value of INFO_ITEM_TYPE may only bethe name of this table.','USER','dbo','TABLE','RM_FOSSIL';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT IMAGE COMPOSITION: This table is used to group individual sections on a log into logical groups or composites. For example, a group may consist of an upper scale, lower scale and log section body.','USER','dbo','TABLE','RM_IMAGE_COMP';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT IMAGE POSITION LOCATION: This table is used to identify the position or location on the image of various events in the image. Positions may be created to identify the start and end of a header section or diagram, to identify depth calibrated points (and their corresponding depths in the well bore) or to identify positions used to correct and account for skew in the image.','USER','dbo','TABLE','RM_IMAGE_LOC';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT IMAGE SEGMENT: This table describes logical segments on an image, such as those on a raster image of a well log. A section may be atomic in nature (header, upper scale, lower scale, log section etc.) or a composite segment ( such as the scalesand log section that are grouped together on the image).','USER','dbo','TABLE','RM_IMAGE_SECT';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT INFORMATION ITEM: Contains technical information that describes the information content of an item captured in the RM module. These information items may be represented in a variety of media and physical renderings. This table is the super-type of an explicit super-sub type table set. Details about various types of information items are captured in the sub-type tables.','USER','dbo','TABLE','RM_INFORMATION_ITEM';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT INFORMATION ITEM COORDINATE QUALITY: This table is used to track various types of quality measures about the spatial component of information or a product that is managed.','USER','dbo','TABLE','RM_INFO_COORD_QUALITY';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT INFORMATION ITEM DATA QUALITY: This table is used to track various types of quality measures about the content of the information or product that is managed.','USER','dbo','TABLE','RM_INFO_DATA_QUALITY';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT INFORMATION ITEM ALIAS: Alternate names, codes, file numbers, reference numbers etc assigned to a product in a managment system, usually for the purposes of identification.','USER','dbo','TABLE','RM_INFO_ITEM_ALIAS';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT BUSINESS ASSOCIATE: use this table to capture relationships with all bsiness associates EXCEPT authors and creators (these should be captured in RM CREATOR). The most common type of business associate in this table would be a contact, such as a contact to obtain permission to view a document, or a contact from whom to obtain an updated version of the information.','USER','dbo','TABLE','RM_INFO_ITEM_BA';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT INFORMATION ITEM CONTENT: this table is used to associate an information item with the PPDM items that comprise it. An information item may be associated with one or more seismic lines, projects, land rights, stratigraphic analysis, fields, pools etc. Note that each association must be populated as a new row of data. Do not try to use one row for more than one association.','USER','dbo','TABLE','RM_INFO_ITEM_CONTENT';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT INFORMATION ITEM DESCRIPTIONS: this table was added so that users may capture any of the meta data elements in an FGDC or ISO metadataset as they require. The most common elements have been mapped into existing tables and columns in PPDM.It is preferable to use the explicitly named tables and columns wherever possible. use this table','USER','dbo','TABLE','RM_INFO_ITEM_DESC';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT INFORMATION ITEM GROUP: Use this table to group information items together, such as all the items that go in a well file, or all the items that are part of an application. You may use this table to group together items that are seperate but significant parts of a composite information item.','USER','dbo','TABLE','RM_INFO_ITEM_GROUP';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT INFORMATION ITEM MAINTENANCE: This table is most commonly used to track update schedules for information items that are updated on a periodic basis through a subscription. Spatial layers are a good example. Physical maintenance, such as tape rewinding schedules or cleanings, should be tracked in RM PHYS ITEM MAINT.','USER','dbo','TABLE','RM_INFO_ITEM_MAINT';
go

execute sp_addextendedproperty 'Description','INFORMATION ITEM ORIGIN: Describes the relationship between information items as they are transformed into new information items through the application of a technical process. For seismic trace data, this may be the conversion of a brute stack to a flattened stack. For field survey data, this may be the conversion of raw survey notes to computed survey notes. For versioned documents, this may be the creation of new updates to an existing document.','USER','dbo','TABLE','RM_INFO_ITEM_ORIGIN';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT INFORMATION ITEM STATUS: use this table to describe the status of an information item from various points of view (update, financial approval, authorship, review etc).','USER','dbo','TABLE','RM_INFO_ITEM_STATUS';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT KEY WORD: this table captures key words from various thesauruses to describe a dataset. As many key words from as many thesaurii may be captured as necessary.','USER','dbo','TABLE','RM_KEYWORD';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT LITHOLOGIC SAMPLE: One of the valid subtypes of INFORMATION ITEM. This may be drilling cuttings, rock samples, mud samples etc. This is a valid sub type of RM INFORMATION ITEM. In this table, the value of INFO_ITEM_TYPE may only be the nameof this table.','USER','dbo','TABLE','RM_LITH_SAMPLE';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT MAP: An item that has been entered into records management that is a map or survey drawing, such as a survey plan, planimetric map or drawings, topographic map, project plan map, interpreted map etc. This is a valid sub type of RM INFORMATION ITEM.In this table, the value of INFO_ITEM_TYPE may only be the name of this table.','USER','dbo','TABLE','RM_MAP';
go

execute sp_addextendedproperty 'Description','PHYSICAL ITEM: Contains information about the physical rendering of an item. A physical item exists at the level to which it is catalog ued or stored. For example, if a file folder is catalogued, it is the phys ical item. The contents of the folder can be described in the many:many relationship between physical and seismic items.','USER','dbo','TABLE','RM_PHYSICAL_ITEM';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT PHYSICAL ITEM CONDITION: Describes specific condition of a physical item, such as a magnetic tape, and actions taken to correct these conditions.','USER','dbo','TABLE','RM_PHYS_ITEM_CONDITION';
go

execute sp_addextendedproperty 'Description','PHYSICAL ITEM GROUP: use this table to define groups of physical items that are combined to create a logical unit. You may want to retrieve all at the same time, or itentify logical groups for visual inspection. For example, a well log may exist as severalscanned images, with the header, scale bars and logs themselves scanned at different resolutions. In order to properly use the items, the entire group must be retrieved.','USER','dbo','TABLE','RM_PHYS_ITEM_GROUP';
go

execute sp_addextendedproperty 'Description','PHYSICAL ITEM MAINTENANCE: Describes maintenance requirements and activities for physical data. For tapes, it describes tape maintenance. Requires further analysis and validation.','USER','dbo','TABLE','RM_PHYS_ITEM_MAINT';
go

execute sp_addextendedproperty 'Description','PHYSICAL ITEM ORIGIN: Describes the origins of different physical renderings or copies of a physical item. For example, if a tape is copied, you can use origin to identify both parent and child items involved in the process.','USER','dbo','TABLE','RM_PHYS_ITEM_ORIGIN';
go

execute sp_addextendedproperty 'Description','PHYSICAL ITEM STORE: A many to many break out entity between physical item and data store. Used only when an object has more than one authorized storage location. The preferred location is kept literally in physical item, with alternate locations in this breakout table. If it is desired, one can keep all locations here, and use the PRIMARY LOCATION in Physical Item to track its current authorized location (as in a tape which has a pla ce both in the data warehouse and in the computer room).','USER','dbo','TABLE','RM_PHYS_ITEM_STORE';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT SEISMIC TRACE ITEM: An item that has been entered into records management that is composed of seismic traces, such as raw field data, processed data or interpreted sections. This is a valid sub type of RM INFORMATION ITEM. In this table, the value of INFO_ITEM_TYPE may only be the name of this table.','USER','dbo','TABLE','RM_SEIS_TRACE';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT SPATIAL DATA SET: This sub type is used to define information items that are spatial datasets, such as hydrography, culture, well data, soil data etc that may be obtained for corporate use. Usually this data is subject to periodic updates, and is described by a set of meta data. The meta data elements have been mapped from FGDC into PPDM, to faciliate loading of this data.','USER','dbo','TABLE','RM_SPATIAL_DATASET';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT THESAURUS: A set of words or descriptors about a particular field or set of concepts, such as a list of subject headings or descriptors usually with a cross-reference system for use in the organization of a collection of documents for reference and retrieval (from Merriam Webster Online)','USER','dbo','TABLE','RM_THESAURUS';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT THESAURUS GLOSSARY: A set of definitions that define a term or word in a thesaurus. For each term there may be one or more glossary defintions, which may be semantically similar or quite different. Regional, cultural or corporate differences are not uncommon.','USER','dbo','TABLE','RM_THESAURUS_GLOSSARY';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT THESAURUS WORD: A term in a thesaurus. Each term may be subject to one or more glossary definitions.','USER','dbo','TABLE','RM_THESAURUS_WORD';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT THESAURUS: A set of words or descriptors about a particular field or set of concepts, such as a list of subject headings or descriptors usually with a cross-reference system for use in the organization of a collection of documents for reference and retrieval (from Merriam Webster Online)','USER','dbo','TABLE','RM_THESAURUS_WORD_XREF';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT SEISMIC TRACE HEADER: This table may be used to capture details in the seismic trace header records, such as the SEG Y header.','USER','dbo','TABLE','RM_TRACE_HEADER';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT WELL LOG or WELL LOG CURVE ITEM: This is a valid sub type of RM INFORMATION ITEM designed to manage well logs and log curves in the records management system. Details about the well log or curve should be stored in the WELL LOG module. Rastor images can be stored in RM FILE CONTENT, references to other physical or digital media are managed in this module as well. This is a valid sub type of RM INFORMATION ITEM. In this table, the value of INFO_ITEM_TYPE may only be the name of this table.','USER','dbo','TABLE','RM_WELL_LOG';
go

execute sp_addextendedproperty 'Description','ACCESS CONDITION CODE: a set of codified instructions regarding access to a business object.','USER','dbo','TABLE','R_ACCESS_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ACCOUNTING PROCEDURE TYPE: The type of accounting procedure , especially in the case where a standard accounting procedure is used. (e.g. PASC 1988 or COPAS 1986)','USER','dbo','TABLE','R_ACCOUNT_PROC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ACTIVITY SET TYPE: The type of activity set, such as standard, corporate, regulatory etc.','USER','dbo','TABLE','R_ACTIVITY_SET_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ACTIVITY: A reference table identifying the type of activit y that caused the movement of fluids to occur such as production, injection, flaring, sales, etc.','USER','dbo','TABLE','R_ACTIVITY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL TREATMENT ADDITIVE METHOD: The method used for adding the additive to the well bore. While the patterns of addition may be complex, use this reference value to describe the method generally.','USER','dbo','TABLE','R_ADDITIVE_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL TREATMENT ADDITIVE TYPE: This reference table identifies the type of additive used in the treatment fluid during the acidizing job. For example, acid, detergent, ChemGel etc.','USER','dbo','TABLE','R_ADDITIVE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ADDRESS TYPE: A reference table identifying valid business associate address types. For example shipping, billing or sales.','USER','dbo','TABLE','R_ADDRESS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE AIR CRAFT TYPE: The type of aircraft described. Examples may be general (jet, two engine, helicopter) or very specific, such as the list found here http://www.airlinecodes.co.uk/acrtypes.htm.','USER','dbo','TABLE','R_AIRCRAFT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE AIR GAS CODE:This reference table identifies the the type of fluid supplied by the drilling compressor. For example, the fluid can be Air or Gas.','USER','dbo','TABLE','R_AIR_GAS_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS REASON TYPE: This reference table describes the purpose or reason for a given alias. For example a well alias may be assigned to the well because of a name change or amendment to the identifier. A business associate alias may be created as a result of a merger or name change.','USER','dbo','TABLE','R_ALIAS_REASON_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS TYPE: This reference table describes the type of alia s. For example a well may be assigned a government alias, contract alias or project alias. Business associates may be assigned a stock exchange symbol alias.','USER','dbo','TABLE','R_ALIAS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALLOCATION FACTOR TYPE: A reference table identifying the t ype of allocation factor that is used in calculations to attribute (al locate) a measured movement of fluid to a number of production entitie s.','USER','dbo','TABLE','R_ALLOCATION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ALLOWABLE EXPENSE: expenses that are allowed under the terms of the contract.','USER','dbo','TABLE','R_ALLOWABLE_EXPENSE';
go

execute sp_addextendedproperty 'Description','REFERENCE OIL ANALYSIS PROPERTY: This reference table identifies the compositional and/or physical properties being analyzed during an oil analysis. For example, the types of properties subjected to analysis may be BTU, Gas composition, Mole percentage,','USER','dbo','TABLE','R_ANALYSIS_PROPERTY';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS ACCURACY TYPE: The kind of accuracy that is indicated, such as the accuracy of transcription from paper or spreadsheet into the database, or the accuracy of the readings that a piece of equipment can make, or the impact of a contaminant into the sample that may render results inaccurate.','USER','dbo','TABLE','R_ANL_ACCURACY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS BUSINESS ASSOCIATE ROLE TYPE: the type of role that a business associate plays or may play during a sample analysis. Examples include technician, scientist, reviewer, laboratory, conducted for, document preparation etc.','USER','dbo','TABLE','R_ANL_BA_ROLE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS CALCULATION EQUIVALENCE TYPE: The kind of relationship or equivalence that is defined for two calculation methods. May indicate methods that provide similar results, methods to be used in preference over another, methods that replace deprecated methods etc.','USER','dbo','TABLE','R_ANL_CALC_EQUIV_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS LIQUID CHROMATOGRAPHY PROPERTY TYPE: The type of chromatography property measured.','USER','dbo','TABLE','R_ANL_CHRO_PROPERTY';
go

execute sp_addextendedproperty 'Description','SAMPLE ANALYSIS COMPONENT TYPE: The type of component assocaited with a sample analysis','USER','dbo','TABLE','R_ANL_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS CONFIDENCE TYPE: The level of confidence or certainty for an analysis value. Various systems for measurment are defined in literature, and may be text based (CERTAIN, PROBABLE, UNCERTAIN) or number based. This value tends to be subjective, and indicates the level of trust the analyst has in the result.','USER','dbo','TABLE','R_ANL_CONFIDENCE_TYPE';
go

execute sp_addextendedproperty 'Description','SAMPLE ANALYSIS DETAIL REFERENCE VALUE TYPE: In the case where a detail is referenced to some other value the type of reference value is captured here. The values, if relevant, are stored in associated columns.','USER','dbo','TABLE','R_ANL_DETAIL_REF_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE SAMPLE ANALYSIS DETAIL TYPE: The type of technical analysis result that has been captured. Note that most common study results are explicitly managed in other detail tables. Use this table only if necessary.','USER','dbo','TABLE','R_ANL_DETAIL_TYPE';
go

execute sp_addextendedproperty 'Description','SAMPLE ANALYSIS ELEMENT VALUE CODE: This table is used to store the code assigned to the value by observation, in cases where numeric values are not used.','USER','dbo','TABLE','R_ANL_ELEMENT_VALUE_CODE';
go

execute sp_addextendedproperty 'Description','SAMPLE ANALYSIS VALUE TYPE: This table is used to describe the type of text values for the analysis.','USER','dbo','TABLE','R_ANL_ELEMENT_VALUE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SAMPLE ANALYSIS EQUIPMENT ROLE: This table contains a list of the valid roles played by equipment during an analysis. May include grinding, polishing, pyrolysis, spectroscopy etc.','USER','dbo','TABLE','R_ANL_EQUIP_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS FORMULA TYPE: The type of formula that has been described. A common kind of calcuation method or formula type are RATIOS.','USER','dbo','TABLE','R_ANL_FORMULA_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS GAS CHROMATOGRAPHY VALUE TYPE: Use this table to list the type of values for the analysis, in cases where the value is text based.','USER','dbo','TABLE','R_ANL_GAS_CHRO_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE GAS ANALYSIS PROPERTY: This reference table identifies the compositional and/or physical properties being analyzed during a gas analysis.','USER','dbo','TABLE','R_ANL_GAS_PROPERTY';
go

execute sp_addextendedproperty 'Description','ANALYSIS PROPERTY VALUE CODE: the code assigned to the analysis property by observation, in cases where numeric values are not used.','USER','dbo','TABLE','R_ANL_GAS_PROPERTY_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS METHOD EQUIVALENCE TYPE: The kind of relationships between analysis methods, indicating whether two methods are exactly the same, nearly the same, a process that supercedes (and hopefully improves) on an older process, a process that is recommended in lieu of another etc.','USER','dbo','TABLE','R_ANL_METHOD_EQUIV_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS METHOD SET TYPE: The type or kind of analysis method set that has been described, such as Isotope analysis, mineral analysis, organic geochemistry, paleontological analysis, bistratigraphic analysis, total organic carbon analysis etc.','USER','dbo','TABLE','R_ANL_METHOD_SET_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS MISSING REPRESENTATION TYPE: A list of valid representations that are used by labs when a measurement is missing because it is out of range. Usually this is the result of equipment limitations.','USER','dbo','TABLE','R_ANL_MISSING_REP';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS NULL REPRESENTATION: a list of the valid values that may be used in the case where a reading or measurement or calculation was not provided.','USER','dbo','TABLE','R_ANL_NULL_REP';
go

execute sp_addextendedproperty 'Description','ANALYSIS PROPERTY VALUE CODE: the code assigned to the analysis property by observation, in cases where numeric values are not used.','USER','dbo','TABLE','R_ANL_OIL_PROPERTY_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS PARAMETER TYPE: this table lists the types of parameters that may be applied to an analysis. Typical examples might be parameters about how much a sample is heated, the type of solvent used, how finely a sample is ground (or sliced) etc. This column may be validated both from the ANL_METHOD table, which controls valid methods and their allowed parameters, or simply through the parameter reference list. Note the controlling the value from the ANL METHOD FK will be more difficult, but will provide a much higher degree of quality support, as only parameters that are valid for the method will be permitted.','USER','dbo','TABLE','R_ANL_PARAMETER_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS PROBLEM RESOLUTION METHOD: The method used to resolve a problem encountered during analysis. Could include re-running the samples, calibrating equipment, collecting a new sample batch, altering parameters and so on.','USER','dbo','TABLE','R_ANL_PROBLEM_RESOLUTION';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS PROBLEM RESULT: The type of consequence that was the outcome of the problem described. For example, results may be inaccurate, or a test may be destroyed, or results may show anomolous values.','USER','dbo','TABLE','R_ANL_PROBLEM_RESULT';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS PROBLEM SEVERITY: A valid type of severity related to a problem encountered during analysis. The severity may range from inconsequential to invalidating the results of a study.','USER','dbo','TABLE','R_ANL_PROBLEM_SEVERITY';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS PROBLEM TYPE: This reference list contains a list of the valid problems that can and are associated with laboratory analysis. Problems may relate to equipment calibration errors, sample contamination, incorrect procedures used, technical error etc.','USER','dbo','TABLE','R_ANL_PROBLEM_TYPE';
go

execute sp_addextendedproperty 'Description','ANALYSYS REFERENCE VALUE TYPE: In the case where a detail is referenced to some other value the type of reference value is captured here. For example, the temperature of a process step may be specific to the atmospheric pressure of the container.','USER','dbo','TABLE','R_ANL_REF_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSYS REMARK TYPE: the kind of remark that has been inserted about a sample analysis. Usually, this would refer to whether this was a comment about the sample, the equipment used, contamination that was found, unusual circumstances that existed etc.','USER','dbo','TABLE','R_ANL_REMARK_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS REPEATABILITY TYPE: The level of repeatability for a study result. Indicates how consistently the same or a similar result will be obtained when a step is repeated. A result may be highly repeatable, but still incorrect or not trustworthy. For example a sample contaminant may affect your trust in the data, even though you get the same (incorrect) answer again and again. Equipment capabilities may also result in a highly repeatable but inaccurate result.','USER','dbo','TABLE','R_ANL_REPEATABILITY';
go

execute sp_addextendedproperty 'Description','STEP CROSS REFERENCE REASON: The reason why two steps are related to each other. Usually would indicate a step that follows another. Could also be used to track a new step that replaces a step that failed or did not have a satisfactory outcome.','USER','dbo','TABLE','R_ANL_STEP_XREF';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS TOLERANCE TYPE: A list of the types of tolerances for valid measurements in an analysis. Tolerances may be related to instrument (equipment) limitations or scientific limits.','USER','dbo','TABLE','R_ANL_TOLERANCE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS MEASUREMENT TYPE: A list of valid measurement types. ANL QC VALID MEASURE lists which measurement types are valid for various types of analysis, and what valid ranges for the values should be. In analysis detail tables, ensure that you have selected a measurement type that is appropriate for the type of study.','USER','dbo','TABLE','R_ANL_VALID_MEASUREMENT';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS VALID MEASUREMENT TYPE: A list of valid measurement types. ANL QC VALID MEASURE lists which measurement types are valid for various types of analysis, and what valid ranges for the values should be. In analysis detail tables, ensure that you have selected a measurement type that is appropriate for the type of study.','USER','dbo','TABLE','R_ANL_VALID_MEAS_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE ANALYSIS FORMULA TYPE: The type of formula that has been described. A common kind of calcuation method or formula type are RATIOS.','USER','dbo','TABLE','R_ANL_VALID_PROBLEM';
go

execute sp_addextendedproperty 'Description','REFERENCE WATERL ANALYSIS PROPERTY: This reference table identifies the compositional and/or physical properties being analyzed during a water analysis.','USER','dbo','TABLE','R_ANL_WATER_PROPERTY';
go

execute sp_addextendedproperty 'Description','REFERENCE ABSOLUTE OPEN FLOW: This reference table identifies the type of Absolute Open Flow procedure. For example, Simplified or Lit procedure.','USER','dbo','TABLE','R_AOF_ANALYSIS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ABSOLUTE OPEN FLOW CALCULATION METHOD: This reference table identifies the type of method used to calculate the absolute open flow potential of the well. For example, single point, multi-point, theoretical or incomplete data.','USER','dbo','TABLE','R_AOF_CALC_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE AMERICAN PETROLEUM INSTITUTE LOG CODE TYPE. A system devised by the American Petroleum Institude published in API bulletin D9 (1973)and D9a (1981) used to classify curves. Often found on historic logs, but rarely used in current operations. This table Identifies which API system was used.','USER','dbo','TABLE','R_API_LOG_SYSTEM';
go

execute sp_addextendedproperty 'Description','APPLICATION COMPONENT TYPE: The type of component associated with the application','USER','dbo','TABLE','R_APPLICATION_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE APPLICATION ATTACHMENT TYPE: the type of appliation attachment that has been sent, such as maps, reports, letters, contracts and so on.','USER','dbo','TABLE','R_APPLIC_ATTACHMENT';
go

execute sp_addextendedproperty 'Description','REFERENCE APPLICATION BUSINESS ASSOCIATE ROLE: This table is used to capture information about the role that a business associate played in the application (approver, creator, reviewer etc.).','USER','dbo','TABLE','R_APPLIC_BA_ROLE';
go

execute sp_addextendedproperty 'Description','LAND RIGHT APPLICATION DECISION: the decision on the applicaiton, such as approved, denied etc.','USER','dbo','TABLE','R_APPLIC_DECISION';
go

execute sp_addextendedproperty 'Description','REFERENCE APPLICATION DESC: The type of descriptive information provided with an application, such as start date, end date, camp location, crew size, equipment type etc.','USER','dbo','TABLE','R_APPLIC_DESC';
go

execute sp_addextendedproperty 'Description','REFERENCE APPLICATION REMARK TYPE: The type of remark about the applicaiton, such as decision remark','USER','dbo','TABLE','R_APPLIC_REMARK_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE APPLICATION STATUS: The status of the application, such as pending, approved, waiting on documents etc.','USER','dbo','TABLE','R_APPLIC_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE APPLICATION TYPE: continuation, groupings, license validations, offset notice appeal, selections, grouping, continuation, significant discovery area, significant discovery license, expiry notification, commercial discovery area, production license.The type of application being made, such as application to drill, application to extend a land right, application to conduct geophysical operations etc.','USER','dbo','TABLE','R_APPLIC_TYPE';
go

execute sp_addextendedproperty 'Description','AREA COMPONENT TYPE: The type of component associated with an area','USER','dbo','TABLE','R_AREA_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE AREA CONTAIN TYPE: A reference to the type of containment, such as a full legal containment, a partial containment (or overlap).','USER','dbo','TABLE','R_AREA_CONTAIN_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE AREA DESCRIPTION CODE: A codified description of an area, such as a project area.','USER','dbo','TABLE','R_AREA_DESC_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE AREA DESCRIPTION TYPE: The type of description of an area, such as size, terrain, vegetation etc.','USER','dbo','TABLE','R_AREA_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE AREA TYPE: The type of area described, such as country, province, basin, project, business area etc.','USER','dbo','TABLE','R_AREA_TYPE';
go

execute sp_addextendedproperty 'Description','R AREA CROSS REFERENCE TYPE: Contains a list of valid reasons for relating areas to each other. These may refer to organizations, jurisdictional relationships etc. Shoud not be used to indicate containment types. Please use AREA_CONTAIN for this purpose.','USER','dbo','TABLE','R_AREA_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','AUTHORITY TYPE: the type of authority given to a business associate, often an employee of a company. Authority may be extended for purchase authorizations, to sign contracts or to enter into negotiations etc.','USER','dbo','TABLE','R_AUTHORITY_TYPE';
go

execute sp_addextendedproperty 'Description','AUTHOR TYPE: the type of author of a document or other product. Could be who the product was created for, the company that created it, the person who created it, the scientist who was in charge etc.','USER','dbo','TABLE','R_AUTHOR_TYPE';
go

execute sp_addextendedproperty 'Description','BUSINESS AUTHORITY COMPONENT TYPE: The type of component associated with the business authority','USER','dbo','TABLE','R_BA_AUTHORITY_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','BA CATEGORY: The category that the business associate is in. For a company, may be legal company, sole proprietorship, corporation etc.','USER','dbo','TABLE','R_BA_CATEGORY';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE COMPONENT TYPE: The type of component associated with a business associate','USER','dbo','TABLE','R_BA_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','R BA CONTACT LOCATION TYPE: The type of contact location defined. May be phone number, fax number, Email address, Web URL etc.','USER','dbo','TABLE','R_BA_CONTACT_LOC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CREW OVERHEAD TYPE: They type of overhead paid to a crew member during a peiod, such as cost of living allowance.','USER','dbo','TABLE','R_BA_CREW_OVERHEAD_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CREW TYPE: A list of valid kinds of crews, such as drilling crews, cleanup crews, inspection crews, logging crews or seismic crews.','USER','dbo','TABLE','R_BA_CREW_TYPE';
go

execute sp_addextendedproperty 'Description','BA DESCRIPTION DETAIL CODE: In the case that the detail is described as a coded value, this table provides the list of valid codes for each type of detail.','USER','dbo','TABLE','R_BA_DESC_CODE';
go

execute sp_addextendedproperty 'Description','BA REFERENCE VALUE TYPE: In the case where a detail is referenced to some other value the type of reference value is captured here. The values, if relevant, are stored in associated columns.','USER','dbo','TABLE','R_BA_DESC_REF_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE BA DESCRIPTION DETAIL TYPE: The kind of detail information about the business associate that has been stored.','USER','dbo','TABLE','R_BA_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','DUE CONDITION: The state that must be achieved for the condition to become effective. For example, a report may be due 60 days after operations commence (or cease).','USER','dbo','TABLE','R_BA_LIC_DUE_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE VIOLATION TYPE: The type of violation of a license that is being recorded. Can be as simple as failure to submit necessary reports or something more difficult such as improper procedures.','USER','dbo','TABLE','R_BA_LIC_VIOLATION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LICENSE VIOLATION RESOLUTION TYPE: The type of resolution to a violation of a license term, such as the payment of a fine or creation of new processes.','USER','dbo','TABLE','R_BA_LIC_VIOL_RESOL';
go

execute sp_addextendedproperty 'Description','BUSINESS ASSOCIATE ORGANIZATION COMPONENT TYPE: The type of component associated with a business associate organization','USER','dbo','TABLE','R_BA_ORGANIZATION_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE BA ORGANIZATION TYPE: may be department, division, cost center, business unit, franchise etc.','USER','dbo','TABLE','R_BA_ORGANIZATION_TYPE';
go

execute sp_addextendedproperty 'Description','BUSINSS ASSOCIATE PERMIT TYPE: the type of permit that the business associate has, such as drilling, seismic exploration etc.','USER','dbo','TABLE','R_BA_PERMIT_TYPE';
go

execute sp_addextendedproperty 'Description','PREFERENCE TYPE: The type of preference documented, such as preference for meeting times, well log curve selection, parameter useage etc.','USER','dbo','TABLE','R_BA_PREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE BUSINESS ASSOCIATE SERVICE TYPE: A valid list of services for a business associate. For example well logger, drilling contractor, application developer. For land, may be may be address for service, brokerage, maintainor etc.','USER','dbo','TABLE','R_BA_SERVICE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE BA STATUS: The current status of the Business Associate, such as Active, In Receivership, Sold, Merged.','USER','dbo','TABLE','R_BA_STATUS';
go

execute sp_addextendedproperty 'Description','BA TYPE: The type of business associate. Usual reference values include COMPANY, PERSON, REGULATORY, SOCIETY, ASSOCIATION. Use the BA CATEGORY to further categorise this information.','USER','dbo','TABLE','R_BA_TYPE';
go

execute sp_addextendedproperty 'Description','R BA XREF TYPE: may be buy-out, name change, merger etc. NOT to be used for the organizational structure, or to track employee/employer relationships (this goes in BA organization)','USER','dbo','TABLE','R_BA_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE BHP METHOD: Code describing the method of measuring the bottom hole pressure (e.g., measured, calculated, etc.)','USER','dbo','TABLE','R_BHP_METHOD';
go

execute sp_addextendedproperty 'Description','BOTTOM HOLE PRESSURE TEST TYPE: This reference table represents the type of bottom hole pressure test conducted on the wellbore. For example, bottom hole static gradient, bottom hole buildup, top hole buildup etc.','USER','dbo','TABLE','R_BH_PRESS_TEST_TYPE';
go

execute sp_addextendedproperty 'Description','DRILL BIT BEARING CONDITION: the condition of the drill bit bearing when it is pulled from the hole, such as worn, broken etc.','USER','dbo','TABLE','R_BIT_BEARING_CONDITION';
go

execute sp_addextendedproperty 'Description','BIT CUTTING STRUCTURE MAJOR DULL CHARACTERISTIC: IADC Roller Bit Dull Grading - major dull characteristics of bit such as BC Broken Cone, LN Lost Nozzle, BT Broken teeth/cutters, LT Lost Teeth/Cutters, BU Balled Up, NO No Major/Other Dull Characteristics, CC Cracked Cone (show cone numbers under location) etc.','USER','dbo','TABLE','R_BIT_CUT_STRUCT_DULL';
go

execute sp_addextendedproperty 'Description','DRILL BIT CUTTING STRUCTURE INNER: IADC Roller Bit Dull Grading - inner 2/3 of bit cutting structure tooth condition. Valid values 0-8 in the IADC standard.','USER','dbo','TABLE','R_BIT_CUT_STRUCT_INNER';
go

execute sp_addextendedproperty 'Description','CUTTING STRUCTURE LOCATION: from the IADC Roller Bit Dull Grading location of cracked or dragged cones. A  All Rows H  Heel Rows M  Middle Rows N  Nose Rows.','USER','dbo','TABLE','R_BIT_CUT_STRUCT_LOC';
go

execute sp_addextendedproperty 'Description','DRILL BIT CUTTING STRUCTURE OUTER: the condition of the outer 1/2 of the tooth, derived from the IADC Roller Bit Dull Grading  outer 1/3 of bit cutting structure tooth condition. Valid values 0-8 in the IADC standard.','USER','dbo','TABLE','R_BIT_CUT_STRUCT_OUTER';
go

execute sp_addextendedproperty 'Description','DRILL BIT REASON PULLED: IADC Roller Bit Dull Grading  reason dull bit pulled such as BHA CHG Bottom Hole Assembly, LOG Run Logs, CD Condition Mud PP Pump Pressure, CP Core Point, PR Penetration Rate.','USER','dbo','TABLE','R_BIT_REASON_PULLED';
go

execute sp_addextendedproperty 'Description','REFERENCE BLOWOUT FLUID: A reference tables describing the type of fluid blown out of a well when a high pressure zone is encountered. For example gas, oil or water.','USER','dbo','TABLE','R_BLOWOUT_FLUID';
go

execute sp_addextendedproperty 'Description','REFERENCE BUILDUP RADIUS:This reference table identifies the magnitude of the buildup radius for the horizontal well. For example, the types of buildup radius can be long, medium or short.','USER','dbo','TABLE','R_BUILDUP_RADIUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CATALOGUE ADDITIVE GROUP: The class or group of additives that this additive belongs to, such as drill mud additive, treatment additive, processing additive etc. Within each group of additives, many types of additives may be described using CAT ADDITIVE TYPE.','USER','dbo','TABLE','R_CAT_ADDITIVE_GROUP';
go

execute sp_addextendedproperty 'Description','REFERENCE CATALOGUE ADDITIVE QUANTITY: The type of quantity in which this particular additive is available, such as sacks, pallets, bales, killograms etc.','USER','dbo','TABLE','R_CAT_ADDITIVE_QUANTITY';
go

execute sp_addextendedproperty 'Description','CATALOGUE ADDITIVE SPECIFICATION TYPE: A list of the kinds of specifications that may be defined for an additive, such as the volume added, weight added, mixing method, preparaation method etc. For each ADDITIVE COMPONENT ID, more than one specificataion could be defined.','USER','dbo','TABLE','R_CAT_ADDITIVE_SPEC';
go

execute sp_addextendedproperty 'Description','ADDITIVE CATALOGUE CROSS REFERENCE TYPE: Use this column to identify the kind of relationship between additives. For example, a new additive may be developed to replace an older product, or two products may be equivalent.','USER','dbo','TABLE','R_CAT_ADDITIVE_XREF';
go

execute sp_addextendedproperty 'Description','CATALOGUE EQUIPMENT GROUP: The functional group of equipment types, such as vehicles, drilling rigs, measuring equipment, monitoring equipment etc. Note that the function of this table may also be assumed by the CLASSIFICATION module for more robust and complete classifications.','USER','dbo','TABLE','R_CAT_EQUIP_GROUP';
go

execute sp_addextendedproperty 'Description','EQUIPMENT CATALOGUE SPECIFICATION TYPE: The type of specification, such as diameter, strength, length, resonating frequency etc. that are listed in the general specifications for a kind of equipment.','USER','dbo','TABLE','R_CAT_EQUIP_SPEC';
go

execute sp_addextendedproperty 'Description','SPECIFICATION TYPE: The type of specification, such as diameter, strength, length, resonating frequency etc.','USER','dbo','TABLE','R_CAT_EQUIP_SPEC_CODE';
go

execute sp_addextendedproperty 'Description','CATALOGUE EQUIPMENT TYPE: the type of equipment that is listed, can be grouped into broad classifications with R CAT EQUIP GROUP if you wish. Note that the function of this table may also be assumed by the CLASSIFICATION module for more robust and complete classifications.','USER','dbo','TABLE','R_CAT_EQUIP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CEMENT TYPE: A reference table identifying the particular type of cement (and additive) used during a cementing operation.','USER','dbo','TABLE','R_CEMENT_TYPE';
go

execute sp_addextendedproperty 'Description','CHECKSHOT SURVEY TYPE: The type of checkshot survey that was conducted to acquire this data, such as VSP, inline checkshot, walkaway checkshot etc.','USER','dbo','TABLE','R_CHECKSHOT_SRVY_TYPE';
go

execute sp_addextendedproperty 'Description','CLASSIFICATION DESCRIPTION PROPERTIES: this table defines the kinds of properties that define levels in a classification system, and also defines how the properties are to be described in CLASS LEVEL DESC.','USER','dbo','TABLE','R_CLASS_DESC_PROPERTY';
go

execute sp_addextendedproperty 'Description','CLASSIFICATION LEVEL COMPONENT TYPE: This table is used to capture the relationships between specific levels of the classification systems and busines objects, such as wells, equipment, documents, seismic sets and land rights. You can also use Classification Systems to embed hierarchies into reference tables, by indicating the name of the reference table that has been classified. In this case, the values in the Classification system should correspond to the values in the reference table (see CLASS LEVEL ALIAS).','USER','dbo','TABLE','R_CLASS_LEV_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','CLASSIFICATION SYSTEM CROSS REFERENCE TYPE: This table may be used to indicate the types of valid relationships between levels of a classification system, such as to establish overlap or equivalence in content, or to indicate the parent(s) of a level.','USER','dbo','TABLE','R_CLASS_LEV_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CLASS SYSTEM DIMENSION: The type of dimension or facet that is in this taxomony or classification system. For example, a taxonomy may exist for an organization, or for geographic areas, or for tools and equipment or materials. By prefrence,taxonomies should contain one dimension or as few dimensions as possible. For classification purposes, each business object can refer to as many classification systems as necessary.','USER','dbo','TABLE','R_CLASS_SYSTEM_DIMENSION';
go

execute sp_addextendedproperty 'Description','CLASSIFICATION SYSTEM CROSS REFERENCE TYPE: Use this table to indicate types of relationships between classification systems. For example, you may indicate that a classification system is approximately the same, or that one is a newer version of another.','USER','dbo','TABLE','R_CLASS_SYST_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','CLIMATE TYPE: A valid type of climate, such as arctic, temperate.','USER','dbo','TABLE','R_CLIMATE';
go

execute sp_addextendedproperty 'Description','REFERENCE COAL RANK SCHEME TYPE: The type of coal rank scheme that is referenced. Could be a formal, recognized scheme, a working scheme etc.','USER','dbo','TABLE','R_COAL_RANK_SCHEME_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CODE VERSION CROSS REFERENCE TYPE: The type of relationship between two reference values in a table, such as equivalent meaning, replacement value, is a kind of and so on.','USER','dbo','TABLE','R_CODE_VERSION_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE COLLAR TYPE:This reference table identifies the type of collar used to couple the tubular with another tubing string.','USER','dbo','TABLE','R_COLLAR_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE COLOR: A list of valid colors. If wished, these colors may be referenced to specific palletts.','USER','dbo','TABLE','R_COLOR';
go

execute sp_addextendedproperty 'Description','R_COLOR_EQUIV - identifies equivalent colors in different palettes.','USER','dbo','TABLE','R_COLOR_EQUIV';
go

execute sp_addextendedproperty 'Description','R COLOR FORMAT: For digital files, the type of color format that has been used. May be expressed as a name (monochrome, greyscale, color) or as a bit value (the number of bits used to reprsent a single pixel of the image. Bi-tonal images have one bit per pixel, 1 BPP. Often RGB images use 24 BPP)','USER','dbo','TABLE','R_COLOR_FORMAT';
go

execute sp_addextendedproperty 'Description','COLOR PALETTE: Identifier for the palette that defines the set of colors in use. Palettes include web safe palettes (216 colors), pantone colors (used for inks) etc.','USER','dbo','TABLE','R_COLOR_PALETTE';
go

execute sp_addextendedproperty 'Description','R WELL COMPLETION METHOD: a reference table identifying the type of aperature through which the fluid entered into the well tubing.','USER','dbo','TABLE','R_COMPLETION_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE COMPLETION STATUS: This reference table defines the type of completion or perforation status. For example, the status can be open, closed, squeezed, plugged, etc.','USER','dbo','TABLE','R_COMPLETION_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE COMPLETION STATUS TYPE: This reference table defines the group or type of status, such as construction, financial, legal etc.','USER','dbo','TABLE','R_COMPLETION_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE COMPLETION TYPE: A reference table identifying the types of well completions or methods. For example perforation, open hole, gravel pack or combination.','USER','dbo','TABLE','R_COMPLETION_TYPE';
go

execute sp_addextendedproperty 'Description','CONDITION TYPE: The type of condition that the physical item is in, such as damaged, unreadable tape, good e','USER','dbo','TABLE','R_CONDITION_TYPE';
go

execute sp_addextendedproperty 'Description','CONFIDENCE TYPE: the type of confidence that is associated with this value. For biostratigraphic analysis, could be confidence in any of the values provided such as the species identification, the diversity count etc.','USER','dbo','TABLE','R_CONFIDENCE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CONFEDENTIALITY REASON: The reason why information or records are confidential, such as legislated confidentialiy period, corporate security etc.','USER','dbo','TABLE','R_CONFIDENTIAL_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE CONFIDENTIALITY TYPE: A reference table that describes the types of confidentiality types usually associated with a well. For example confidential, non-confidential or confidential 90 days.','USER','dbo','TABLE','R_CONFIDENTIAL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CONFORMITY RELATION: A descriptor assigned to formation picks that identifies the type of conformity relationship that describes the surface that was picked. May be unconformity, disconformity, angular unconformity, conformable ro paraconformable. An unconformity is a substantial break or gap in the geologic record wher a rock unit is overlain by another that is not next in stratigraphic succession. Normally implies uplift or erosion with loss of the previously formed strata.','USER','dbo','TABLE','R_CONFORMITY_RELATION';
go

execute sp_addextendedproperty 'Description','CONSENT BUSINESS ASSOCIATE ROLE: Describes the role played by a business associate in obtaining a consent, such as signing authority, chief negotiator etc.','USER','dbo','TABLE','R_CONSENT_BA_ROLE';
go

execute sp_addextendedproperty 'Description','CONSENT COMPONENT TYPE: The type of component associated with a consent','USER','dbo','TABLE','R_CONSENT_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','CONSENT CONDITION: A condition that has been imposed as a result of the consent granted. Each condition is based on the condition type, so that a set of conditions for road access may be kept seperate from conditions for dock usage.','USER','dbo','TABLE','R_CONSENT_CONDITION';
go

execute sp_addextendedproperty 'Description','CONSENT REMARK TYPE: a code classifying the remark or type of remark.','USER','dbo','TABLE','R_CONSENT_REMARK';
go

execute sp_addextendedproperty 'Description','CURRENT CONSENT STATUS: the current status of this consent such as approved, pending, denied, waiting for report etc.','USER','dbo','TABLE','R_CONSENT_STATUS';
go

execute sp_addextendedproperty 'Description','CONSENT TYPE: the type of consent sought, such as proximity consent, crossing consent, trapper consent, road use agreement.','USER','dbo','TABLE','R_CONSENT_TYPE';
go

execute sp_addextendedproperty 'Description','CONSULTATION ATTENDANCE TYPE: the type of attendance at a discussion, such as regrets, in person, by phone connection, represented in written document etc.','USER','dbo','TABLE','R_CONSULT_ATTEND_TYPE';
go

execute sp_addextendedproperty 'Description','CONSULTATION COMPONENT TYPE: the type of component that is associated with the consultation. Could be a land right, seismic set, contract, facility etc.','USER','dbo','TABLE','R_CONSULT_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','CONSULTATION DISCUSSION TYPE: the nominal type of discussion that was held, such as phone, mail, email, chat or in person.','USER','dbo','TABLE','R_CONSULT_DISC_TYPE';
go

execute sp_addextendedproperty 'Description','CONSULTATION ISSUE TYPE: A list of valid consultation detail types. Details may include the issues that are raised or resolved etc.','USER','dbo','TABLE','R_CONSULT_ISSUE_TYPE';
go

execute sp_addextendedproperty 'Description','CONSULTATION REASON: the reason the consultation has been undertaken. Could be to obtain compliance with a specific regulation or to negotiate a contract etc.','USER','dbo','TABLE','R_CONSULT_REASON';
go

execute sp_addextendedproperty 'Description','CONSULTATION RESOLUTION: a valid type of resolution to an issue raised in consultation, such as built fence, purchase equipment, provide training.','USER','dbo','TABLE','R_CONSULT_RESOLUTION';
go

execute sp_addextendedproperty 'Description','CONSULTATION BA ROLE: A list of valid roles that can be played by participants in a consultation. Examples include counsil, observer, initiator etc.','USER','dbo','TABLE','R_CONSULT_ROLE';
go

execute sp_addextendedproperty 'Description','CONSULTATION TYPE: A list of valid consultation types that are undertaken. Could be for negotiating a benefits agreement, obtaining surface access, use of a support facility etc.','USER','dbo','TABLE','R_CONSULT_TYPE';
go

execute sp_addextendedproperty 'Description','CONSULTATION CROSS REFERENCE TYPE: the type of consultation relationship that exists. For example, a consultation may be a component of a larger consultation project, or can replace or supplement another consultation.','USER','dbo','TABLE','R_CONSULT_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE BA INTEREST SET PARTNER CONTACT ROLE: the role played by the contact for the partner in the interest set, such as negotiator, authorization, legal representative etc.','USER','dbo','TABLE','R_CONTACT_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE CONTAMINANT TYPE: This reference table describes the type of contaminant that may be present in a well test recovery or sample analysis. For example corrosive gases.','USER','dbo','TABLE','R_CONTAMINANT_TYPE';
go

execute sp_addextendedproperty 'Description','CONTEST COMPONENT TYPE: The type of component associated with a contest','USER','dbo','TABLE','R_CONTEST_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CONTEST PARTY ROLE: the role the party played in the contest, such as mediator, plaintiff, defendant, arbitrator etc.','USER','dbo','TABLE','R_CONTEST_PARTY_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE CONTEST RESOLUTION METHOD: the method used to arrive at the resolution of the land right contest, such as binding arbitration, court ruling, mutual accord etc.','USER','dbo','TABLE','R_CONTEST_RESOLUTION';
go

execute sp_addextendedproperty 'Description','R CONTEST TYPE: The type of contest oversuch as a land ownership or rights dispute. .','USER','dbo','TABLE','R_CONTEST_TYPE';
go

execute sp_addextendedproperty 'Description','CONTRACT COMPONENT TYPE: The type of component associated with a contract','USER','dbo','TABLE','R_CONTRACT_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CONTRACT BUSINESS ASSOCIATE ROLE: A role that is played by a business associate for the support of a contract.','USER','dbo','TABLE','R_CONT_BA_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE CONTRACT COMPONENT REASON TYPE: The reason why the component is associated with the contract, such as acquired under terms of the contract, governed by the contract, part of litigation process etc.','USER','dbo','TABLE','R_CONT_COMP_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE EXTENSION CONDITION: The method by which the contract may be managed or extended over its life time. For example, a contract may be held by production, held for the life of the lease, evergreen (goes year to year until one party terminates) ormust be renegotiated at the end of the primary term. In some cases, specific conditions must be met for the contract to extend past the primary term.','USER','dbo','TABLE','R_CONT_EXTEND_COND';
go

execute sp_addextendedproperty 'Description','CONTRACT EXTENSION TYPE: the type of extension that has been granted for the contract. May be based on production status, statute, contract conditions etc.','USER','dbo','TABLE','R_CONT_EXTEND_TYPE';
go

execute sp_addextendedproperty 'Description','INSURANCE ELECTION: All parties of the contract agree that they are self insured, and additional coverage is not necessary. This means that if there is an actionable problem during operations, the operator may be required to pay own legal costs without recourse to reimbursement. Could also be that the Operator is insured.','USER','dbo','TABLE','R_CONT_INSUR_ELECT';
go

execute sp_addextendedproperty 'Description','REFERENCE OPERATING PROCEDURE CODE: the version of a standard operating procedure that you are using.','USER','dbo','TABLE','R_CONT_OPERATING_PROC';
go

execute sp_addextendedproperty 'Description','REFERENCE CONTRACT PROVISION TYPE: A list of values for types of contract provisions (e.g. EARNING, POOLED INTERESTS, etc.)','USER','dbo','TABLE','R_CONT_PROVISION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CONTRACT PROVISION CROSS REFERENCE TYPE: a list of types of relationships between contract provisions, such as when an stipulation in one contract overrides another stipulation, or to refer to other relevant information. (e.g. a royalty agreement which stipulates who the royalty is paid to but the parties who pay the royalty change by virtue of a joint operating agreement)','USER','dbo','TABLE','R_CONT_PROV_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CONTRACT STATUS: a list of valid status types for a contract, such as active, inactive, pending, terminated, draft etc','USER','dbo','TABLE','R_CONT_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE CONTRACT STATUS TYPE: a list of valid status types for a contract, such as legal status, negotiation status, financial status etc.','USER','dbo','TABLE','R_CONT_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CONTRACT TYPE: List of valid types of contract, such as pooling agreement, joint venture, joint operating agreement, farm-out.','USER','dbo','TABLE','R_CONT_TYPE';
go

execute sp_addextendedproperty 'Description','CONTRACT VOTING RESPONSE: the types of response allowed for a vote. Usually three responses (abstain, for, against). Alternate terms may be used, such as yes / no, positive / negative, agree / disagree etc.','USER','dbo','TABLE','R_CONT_VOTE_RESPONSE';
go

execute sp_addextendedproperty 'Description','CONTRACT VOTING PROCEDURE TYPE: The type of voting procedure that is captured, such as general operations, enlargment, exhibits.','USER','dbo','TABLE','R_CONT_VOTE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CONTRACT CROSS REFERENCE TYPE: The type of relationship between two contracts, such as supercedence or governing relationship. Note that relationships between contract provisions is captured in the table CONT PROVISION XREF.','USER','dbo','TABLE','R_CONT_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE COORDINATE CAPTURE METHOD: A reference table identifying valid methods of capturing coodinate data. For example: Digitizing, Surveying, ...','USER','dbo','TABLE','R_COORD_CAPTURE';
go

execute sp_addextendedproperty 'Description','REFERENCE COORDINATE COMPUTATION METHOD: A reference table identifying valid methods of computing coordinate values. For example: ATS21 (using bilinear interpolation and the Alberta Township System Version 2.1 grid nodes.)','USER','dbo','TABLE','R_COORD_COMPUTE';
go

execute sp_addextendedproperty 'Description','REFERENCE COORDINATE QUALITY: the quality of the coordiante, such as validated, unvalidated, poor etc.','USER','dbo','TABLE','R_COORD_QUALITY';
go

execute sp_addextendedproperty 'Description','COORDINATE SYSTEM TYPE: the type of coordinate system. Will include Geographic coordinate system, local spatial coordinate system, Geocentric coordinate system, Map Grid coordinate system, and vertical coordinate system.','USER','dbo','TABLE','R_COORD_SYSTEM_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CORE HANDLING: A reference table identifying the type of technique used to preserve the core. For example, wrapped in plastic or fibreglass sleeve.','USER','dbo','TABLE','R_CORE_HANDLING';
go

execute sp_addextendedproperty 'Description','REFERENCE CORE RECOVERY TYPE: This table identifies the type of core recovery. For sidewall cores the values may be recovered, lost or misfired.','USER','dbo','TABLE','R_CORE_RECOVERY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERNECE CORE SAMPLE: This reference table identifies the type of core sample. The core sample may be a full diameter (whole core) or a plug sample (button, plug, cutting).','USER','dbo','TABLE','R_CORE_SAMPLE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CORE SHIFT METHOD: This reference table defines the method used to correct core depths to adjusted wireline log depths.','USER','dbo','TABLE','R_CORE_SHIFT_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE CORE SOLVENT: This reference table describes the solvent used for removing residual fluids from the core. For example, a common fluid used for distillation-extraction is toluene.','USER','dbo','TABLE','R_CORE_SOLVENT';
go

execute sp_addextendedproperty 'Description','REFERENCE CORE TYPE: A reference table which defines the type of core procedure used during the coring operation. For example, conventional, sidewall, diamond, triangle etc..','USER','dbo','TABLE','R_CORE_TYPE';
go

execute sp_addextendedproperty 'Description','CORRECTION METHOD: the correction method used to repair damage done to a physical item.','USER','dbo','TABLE','R_CORRECTION_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE COUPLING TYPE: A short length of pipe used to connect two joints of casing. A casing coupling has internal threads (female threadform) machined to match the external threads (male threadform) of the long joints of casing. The two joints of casing are threaded into opposite ends of the casing coupling. Synonyms: casing collar (Schlumberger Oilfield Glossary)','USER','dbo','TABLE','R_COUPLING_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CREATOR TYPE: The type of creatorship of a document, report or other object. Could be primary author, corporate author, scientific author, laboratory, field collection etc','USER','dbo','TABLE','R_CREATOR_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE TRANSFORM PARAMETERS: A valid transform parameter that may be applied during a conversion between coordinate systems.','USER','dbo','TABLE','R_CS_TRANSFORM_PARM';
go

execute sp_addextendedproperty 'Description','REFERENCE TRANSFORM TYPE: A reference table identifying valid Geodetic Transformation types. For example, Bursa-Wolfe, Molodensky, Cartesian, Geocentric or Grid.','USER','dbo','TABLE','R_CS_TRANSFORM_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CURVE SCALE: This reference table identifies the type of curve scale. For example, the valid codes may be straight, shift, X5 or X10.','USER','dbo','TABLE','R_CURVE_SCALE';
go

execute sp_addextendedproperty 'Description','REFERENCE LOG CURVE TYPE: This reference table identifies the type of wireline log curve recorded during the logging operation. For example, caliper, gamma ray.','USER','dbo','TABLE','R_CURVE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LOG CURVE CROSS REFERENCE TYPE: The level of the cross reference that is captured. As this is a breakout table, you have the option of capturing each level in a hierarchy explicitly, so that relationships may be parent to child, grandparent to child, great grandparent to child etc. Other types of relationships may also be defined here.','USER','dbo','TABLE','R_CURVE_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CUSHION TYPE: A reference table identifying the type of cushion used during a well test. For example water, nitrogen, ammonia, or carbon dioxide.','USER','dbo','TABLE','R_CUSHION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE CUTTING FLUID: This table identifies the type of fluid used to cut the core into samples.','USER','dbo','TABLE','R_CUTTING_FLUID';
go

execute sp_addextendedproperty 'Description','PROCESS DONE: This process would be used by Records managers to describe a records managment process, such as pulled, shipped etc.','USER','dbo','TABLE','R_DATA_CIRC_PROCESS';
go

execute sp_addextendedproperty 'Description','STATUS: Status of the item, such as checked in or out.','USER','dbo','TABLE','R_DATA_CIRC_STATUS';
go

execute sp_addextendedproperty 'Description','DATA STORE TYPE: the type of data store that is referenced, such as disk, folder, tape, shelf, SAN system server or optical disk.','USER','dbo','TABLE','R_DATA_STORE_TYPE';
go

execute sp_addextendedproperty 'Description','DATE FORMAT TYPE: The type of date format used in this table, such as YYYY or YYYYQQ or YYYYMM or YYYYMMDD etc. Indicates the degree of accuracy in the dates.','USER','dbo','TABLE','R_DATE_FORMAT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE GEODETIC DATUM ORIGIN: A reference table identifying the valid origins for Geodetic Datums. For example, Geocentric, Local Origin, Local Meridian.','USER','dbo','TABLE','R_DATUM_ORIGIN';
go

execute sp_addextendedproperty 'Description','PRODUCTION DECLINE CURVE CONDITION CODE: a validated set of codes that may be associated with certain types of decline condition types. Note that only some condition types will have codes. Others will be associated with numberic or text descriptions only.Codes may be used to indicate whether the number of wells includes producing wells, injecting wells or both for example.','USER','dbo','TABLE','R_DECLINE_COND_CODE';
go

execute sp_addextendedproperty 'Description','PRODUCTION DECLINE CURVE CONDITION TYPE: The type of condition that is described for the production decline analysis, such as the number of producing oil wells, number of injection wells, service factors etc.','USER','dbo','TABLE','R_DECLINE_COND_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE DECLINE CURVE TYPE: A reference table identifying the type of decline curve that is used in decline curve forecast calculations such as exponential, harmonic, hyperbolic, linear, etc.','USER','dbo','TABLE','R_DECLINE_CURVE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE DECLINE TYPE: A reference table identifying the type of decline that is used in decline curve forecast calculations such as nominal or effective percentatge.','USER','dbo','TABLE','R_DECLINE_TYPE';
go

execute sp_addextendedproperty 'Description','DECRYPTION TYPE: the type of decryption that must be done to this file to get at the data. Examples include unzip, untar, run a specified procedure etc.','USER','dbo','TABLE','R_DECRYPT_TYPE';
go

execute sp_addextendedproperty 'Description','R DEDUCTION TYPE: the type of decution to be made to a payment, such as tax, CPP, provincial tax, state tax, federal tax.','USER','dbo','TABLE','R_DEDUCT_TYPE';
go

execute sp_addextendedproperty 'Description','DIGITAL FORMAT: The digital format, or predefined order or arrangement of digits. For trace data, may be SEG Y or SEG B, for survey data may be UKOOA or SEG P1 etc.','USER','dbo','TABLE','R_DIGITAL_FORMAT';
go

execute sp_addextendedproperty 'Description','REFERENCE DIGITAL OUTPUT: The format that a parameter is to be output as when reporting or recreating a digital file.','USER','dbo','TABLE','R_DIGITAL_OUTPUT';
go

execute sp_addextendedproperty 'Description','DIRECTION: a set of valid compass directions, used for referencing positional information. For example, N, S, NE etc.','USER','dbo','TABLE','R_DIRECTION';
go

execute sp_addextendedproperty 'Description','REFERENCE TABLE DIRECTIONAL SURVEY ACCURACY REASON: this table provides a standard lists of the reasons why station accuracy may be affected in a directional survey.','USER','dbo','TABLE','R_DIR_SRVY_ACC_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE DIRECTIONAL SURVEY CLASS: A reference table identifying valid classes of directional surveys. For example, directional survey where both inclination and azimuth measured, hole deviation where only inclination measured.','USER','dbo','TABLE','R_DIR_SRVY_CLASS';
go

execute sp_addextendedproperty 'Description','REFERENCE ALIAS DIRECTIONALSURVEY COMPUTATION METHOD: A reference table identifying the valid methods used to compute a directional survey. For example, Radius of Curvature, Minimum Curvature, Balanced Tangential,...','USER','dbo','TABLE','R_DIR_SRVY_COMPUTE';
go

execute sp_addextendedproperty 'Description','DIRECTIONAL SURVEY CORRECTIONAL ANGLE TYPE:   The type of correction angle used to correct the directional survey tool from the native north or east measurements to the surveys projected reference.','USER','dbo','TABLE','R_DIR_SRVY_CORR_ANGLE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE DIRECTIONAL SURVEY POINT TYPE: A reference table identifying the valid ways survey measurements are obtained for a directional survey observation point. For example, measured, extrapolated beyond a survey, interpolated between t wo existing survey points.','USER','dbo','TABLE','R_DIR_SRVY_POINT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE DIRECTIONAL SURVEY PROCESS METHOD: The processing method of the reported (original hardcopy, PDF, etc.) data: interpolated, non-interpolated or mixed.','USER','dbo','TABLE','R_DIR_SRVY_PROCESS_METH';
go

execute sp_addextendedproperty 'Description','REFERENCE TABLE DIRECTIONAL SURVEY RADIUS OF UNCERTAINTY: A reference table to indicate reasons and valid values related to radius of uncertainty.','USER','dbo','TABLE','R_DIR_SRVY_RAD_UNCERT';
go

execute sp_addextendedproperty 'Description','REFERENCE DIRECTIONAL SURVEY RECORD MODE: A reference table identifying valid record modes for Directional Surveys. For example, multi shot, single shot.','USER','dbo','TABLE','R_DIR_SRVY_RECORD';
go

execute sp_addextendedproperty 'Description','DIRECTIONAL SURVEY REPORT TYPE: the kind of directional survey report, such as run, combined run, composite etc.','USER','dbo','TABLE','R_DIR_SRVY_REPORT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE DIRECTIONAL SURVEY TYPE: A reference table identifying valid types of Directional Surveys. For example, gyroscopic, magnetic, MWD, hole deviation, totco, acid bottle, ...','USER','dbo','TABLE','R_DIR_SRVY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE DISTANCE REFERENCE POINT: A reference table for the location name or reference point for measurement of distance to an object (e.g. offshore well) or point. Examples: Cape Fear, Port of Aberdeen, Jonesville.','USER','dbo','TABLE','R_DIST_REF_PT';
go

execute sp_addextendedproperty 'Description','DOCUMENT TYPE: The type of document. ie: monthly periodical or technical publication or specificiation. Could also be a report or analysis.','USER','dbo','TABLE','R_DOCUMENT_TYPE';
go

execute sp_addextendedproperty 'Description','DOCUMENT STATUS: The status of the document. Can include whether the document has been executed, marked as draft etc.','USER','dbo','TABLE','R_DOC_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE DRILLING MEDIA: A reference table identifying the various drilling media type present in a wellbore. Commonly refered to as MUD TYPE. For example chemical gel mud, crude oil or native mud.','USER','dbo','TABLE','R_DRILLING_MEDIA';
go

execute sp_addextendedproperty 'Description','DRILL ASSEMBLY COMPONENT TYPE: A reference table describing the type of component that has been placed on the assembly. Specific equipment parameters, manufacturer information etc may be captured by using the EQUIPMENT module.','USER','dbo','TABLE','R_DRILL_ASSEMBLY_COMP';
go

execute sp_addextendedproperty 'Description','DRILL BIT CONDITION: the condition of the drill bit when it is pulled from the hole, such as worn, broken etc.','USER','dbo','TABLE','R_DRILL_BIT_CONDITION';
go

execute sp_addextendedproperty 'Description','DRILL BIT DETAILCODE: this table captures the kinds of codes needed to describe each kind of detail record, in the case where the value captured in not numeric. For example, you may wish to track whether a sensor is ON or OFF.','USER','dbo','TABLE','R_DRILL_BIT_DETAIL_CODE';
go

execute sp_addextendedproperty 'Description','DRILL BIT DETAILTYPE: Use this table to capture any additional detail about the bit and its condition or use that is not otherwise captured in the model.','USER','dbo','TABLE','R_DRILL_BIT_DETAIL_TYPE';
go

execute sp_addextendedproperty 'Description','DRILL BIT JET TYPE: Describes the type of jet used - for instance, Standard, Short Extended, etc.','USER','dbo','TABLE','R_DRILL_BIT_JET_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE DRILLING BIT TYPE: This reference table identifies the type of drilling bit used to drill the wellbore segment.','USER','dbo','TABLE','R_DRILL_BIT_TYPE';
go

execute sp_addextendedproperty 'Description','DRILLING HOLE POSITION: The location on a vessel that describes the position of the drilling hole through which operations proceed.','USER','dbo','TABLE','R_DRILL_HOLE_POSITION';
go

execute sp_addextendedproperty 'Description','REFERENCE DRILL REPORT TIME: a list of the valid points in a reporting period, shift or tour when measurements and records are made. Usually, this is done at the start or end of the shift.','USER','dbo','TABLE','R_DRILL_REPORT_TIME';
go

execute sp_addextendedproperty 'Description','DRILLING STATISTIC CODE: Use this table to define valid statistic or metrics values where the value is selected from a list. Each type of statistic may contain its own set of valid codes using this table.','USER','dbo','TABLE','R_DRILL_STAT_CODE';
go

execute sp_addextendedproperty 'Description','DRILLING STATISTIC TYPE: Use this table to define valid well operations or drilling statistics that are not explicitly defined in the PPDM data model. Use the PPDM PROPERTY SET and PPDM PROPERTY COLUMN tables to define how each kind of statistic should be captured.','USER','dbo','TABLE','R_DRILL_STAT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE DRILL TOOL TYPE: This reference tables describes the types of drill tools. For example cable or rotary.','USER','dbo','TABLE','R_DRILL_TOOL_TYPE';
go

execute sp_addextendedproperty 'Description','ECONOMIC SCENARIO: A reference table identifying the economic scenarios which have been set up to allow economics to be run under multiple pricing and operating cost assumptions (scenarios).','USER','dbo','TABLE','R_ECONOMIC_SCENARIO';
go

execute sp_addextendedproperty 'Description','ECONOMIC SCHEDULE: a reference table that contains a list of the types of values that are described in the economics run. Future versions of the model may re-engineer this table to support additional functionality.','USER','dbo','TABLE','R_ECONOMIC_SCHEDULE';
go

execute sp_addextendedproperty 'Description','ECOZONE HIERARCHY LEVEL: Indicates whether the relationship between parent and child is parent child, grandparent child (two levels apart), great grandparent (3 levels apart) etc. Used for implemnetations who choose to populate all levels of a hierarchy explicitly and avoid the need to query using connect by syntax.','USER','dbo','TABLE','R_ECOZONE_HIER_LEVEL';
go

execute sp_addextendedproperty 'Description','REFERENCE ECOZONE TYPE: the type of ecozone that has been referenced, such as marine, terrestrial, lake atmospheric etc.','USER','dbo','TABLE','R_ECOZONE_TYPE';
go

execute sp_addextendedproperty 'Description','ECOZONE CROSS REFERENCE TYPE: the type of cross reference between ecozones, such as superceded, replacement etc.','USER','dbo','TABLE','R_ECOZONE_XREF';
go

execute sp_addextendedproperty 'Description','EMPLOYEE POSITION: A reference table listing valid types of employee positions. This list may come from human resource departments.','USER','dbo','TABLE','R_EMPLOYEE_POSITION';
go

execute sp_addextendedproperty 'Description','EMPLOYEE STATUS: a reference table listing valid values for the status of an employee or consultant. May be consultant, hourly, on leave, active, retired etc. This list may be derived from the human resource department.','USER','dbo','TABLE','R_EMPLOYEE_STATUS';
go

execute sp_addextendedproperty 'Description','ENCODING TYPE: The type of encoding that has been applied to a digital file. May include security encryption, zipping or other compression, RODE and so on.','USER','dbo','TABLE','R_ENCODING_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ENHANCED RECOVERY TYPE: A reference table identifying the t ypes of method used for enhanced recovery of petroleum substances.','USER','dbo','TABLE','R_ENHANCED_REC_TYPE';
go

execute sp_addextendedproperty 'Description','ENTITLEMENT ACCESS TYPE: the type of access entitlement that is described in the row, such as read, write, delete for database access. For other types of access may be copy, view, sell etc.','USER','dbo','TABLE','R_ENT_ACCESS_TYPE';
go

execute sp_addextendedproperty 'Description','ENTITLEMENT COMPONENT TYPE: the type of entitlement component, or the reason why a business object is associated with this entitlement. For example, a contract may be associated because it governs the conditions of the entitlement, or a seismic set may beassociated because access to its acquisition products are controled by the entitlement.','USER','dbo','TABLE','R_ENT_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','ENTITLEMENT EXIRY ACTION: an action that must occur after the entitlement has expired. For example all copies of the relevant data must be destroyed.','USER','dbo','TABLE','R_ENT_EXPIRY_ACTION';
go

execute sp_addextendedproperty 'Description','REFERENCE ENTITLEMENT SECURITY GROUP TYPE: The kind of security group that has been created, such as reference table updaters, land administrators, project teams, committees etc.','USER','dbo','TABLE','R_ENT_SEC_GROUP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ENTITLEMENT SECURITY GROUP CROSS REFERENCE: The type of relationship between groups, such as a group that governs another, or is part of another, turns into another, or replaces another or works in conjunction with (perhaps with a slightly different role).','USER','dbo','TABLE','R_ENT_SEC_GROUP_XREF';
go

execute sp_addextendedproperty 'Description','ENTITLEMENT TYPE: the type of entitlement that is described in the row, such as a seismic lease data entitlement, a security based entitlement etc.','USER','dbo','TABLE','R_ENT_TYPE';
go

execute sp_addextendedproperty 'Description','ENVIRONMENT TYPE: the environment in which operations occur or data is collected (marine, land, transition)','USER','dbo','TABLE','R_ENVIRONMENT';
go

execute sp_addextendedproperty 'Description','EQUIPMENT BUSINESS ASSOCIATE ROLE TYPE: The role of the business associate, such as rentor, owner, operator, authorized maintenance etc.','USER','dbo','TABLE','R_EQUIP_BA_ROLE_TYPE';
go

execute sp_addextendedproperty 'Description','EQUIPMENT COMPONENT TYPE: The type of component associated with a piece of equipment','USER','dbo','TABLE','R_EQUIP_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','TYPICAL EQUIPMENT INSTALLATION TYPE: Indicates where this type of equipment would normally be used, such as on the drilling assembly, in the well bore, on well site, on rig, in processing facility.','USER','dbo','TABLE','R_EQUIP_INSTALL_LOC';
go

execute sp_addextendedproperty 'Description','MAINTENANCE LOCATION TYPE: Whether the maintenance activity was conducted on site, off site or in some specified location (Delaware warehouse) or type of location (such a maintenance yard).','USER','dbo','TABLE','R_EQUIP_MAINT_LOC';
go

execute sp_addextendedproperty 'Description','REFERENCE MAINTENANCE REASON: The reason why this maintenance activity was undertaken, such as preventative maintenance, predictive maintenance, failure etc.','USER','dbo','TABLE','R_EQUIP_MAINT_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE EQUIPMENT MAINTENANCE STATUS: The status of a maintenance event for a piece of equipment, such as a pump. the status is described in a specific context (MAINT STATUS TYPE), such as financial, operational, or preventative.','USER','dbo','TABLE','R_EQUIP_MAINT_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE MAINTAIN STATUS TYPE: the type of status, or perspective, from which the status of a maintenance event is viewed, such as operational, financial etc.','USER','dbo','TABLE','R_EQUIP_MAINT_STAT_TYPE';
go

execute sp_addextendedproperty 'Description','EQUIPMENT REMOVAL REASON: The reason why this particular piece of equipment was replaced, such as replace due to wear and tear (scheduled), replace due to failure, upgrade.','USER','dbo','TABLE','R_EQUIP_REMOVE_REASON';
go

execute sp_addextendedproperty 'Description','EQUIPMENT SPECIFICATION: Use this table to capture the specification or callibration type of measurement that is captured for a specific piece of equipment.','USER','dbo','TABLE','R_EQUIP_SPEC';
go

execute sp_addextendedproperty 'Description','EQUIPMENT SPECIFICATION REFERENCE TYPE: Use this table to indicate the kind of referece point used to describe specifications. For example, if the specifications being captured are tank strappings, the SPEC TYPE = TANK STRAPPING and SPEC REF TYPE = STRAPPING MARKERS and the REFERENCED VALUE = the height measure on the tank.','USER','dbo','TABLE','R_EQUIP_SPEC_REF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE EQUIPMENT SPECIFICATION SET TYPE:  Use this table to record  the kinds of specification set that are created.  Includes sets for pump callibrations etc.  May also include sets of required specifications for companies, regulatory authorities etc.','USER','dbo','TABLE','R_EQUIP_SPEC_SET_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE EQUIPMENT STATUS or CONDITION TYPE: a reference list of the status or condition types valid for a peice of equipment.','USER','dbo','TABLE','R_EQUIP_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE EQUIPMENT STATUS TYPE: A list of valid types for classifying or grouping status information. Can include financial, operational condition etc.','USER','dbo','TABLE','R_EQUIP_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE EQUIPMENT SYSTEM CONDITION: A list of conditions that equipment must be in for maintenance to occur, such as shut down, moved to repair yard etc.','USER','dbo','TABLE','R_EQUIP_SYSTEM_CONDITION';
go

execute sp_addextendedproperty 'Description','EQUIPMENT USE STATISTIC TYPE: Use statistics for equipment are widely varied in nature, depending on the type of equipment you are tracking. You may need to track distance driven, distance drilled, total revolutions, total cost of operations etc.','USER','dbo','TABLE','R_EQUIP_USE_STAT_TYPE';
go

execute sp_addextendedproperty 'Description','EQUIPMENT CROSS REFERENCE TYPE: the type of relationship between two pieces of equipment, often indicating one piece that can or has replaced another. May also be used to indicate equipment that has the same function, and are therefore equivalent. May be used to indicate the installation of one piece of equipment with or in another.','USER','dbo','TABLE','R_EQUIP_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE EAST-WEST DIRECTION: A reference table identifying valid East-West directions. For example, East, West.','USER','dbo','TABLE','R_EW_DIRECTION';
go

execute sp_addextendedproperty 'Description','REFERENCE EAST WEST START LINE: A reference table identifying valid east-west starting lines for offset distances. This is used primarily for non-orthonormal survey blocks such as Texas surveys and California blocks. For example, FEL first east line, NEL northmost east line,...','USER','dbo','TABLE','R_EW_START_LINE';
go

execute sp_addextendedproperty 'Description','FACILITY CLASSIFICATION TYPE: The type of classification assigned to the facility, such as sour gas. Often has a bearing on environmental restrictions and requirements.','USER','dbo','TABLE','R_FACILITY_CLASS';
go

execute sp_addextendedproperty 'Description','FACILITY COMPONENT TYPE: The type of component associated with a facility','USER','dbo','TABLE','R_FACILITY_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','FACILITY SPECIFICATION CODE: A code for a specification where the result is a text string, rather than a number, and the text string should be validated against a list of values. General narrative descriptions can be stored in FACILITY_DESCRIPTION.SPEC_DESC.','USER','dbo','TABLE','R_FACILITY_SPEC_CODE';
go

execute sp_addextendedproperty 'Description','FACILITY SPECIFICATION TYPE: Use this table to capture the specification measurement type that is captured for a specific facility. Try not to get confused with equipment specifications.','USER','dbo','TABLE','R_FACILITY_SPEC_TYPE';
go

execute sp_addextendedproperty 'Description','FACILITY STATUS: The status of the facility, such as ACTIVE, PENDING, DECOMMISSIONED etc. Defined in terms of a type of status','USER','dbo','TABLE','R_FACILITY_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE FACILITY TYPE: A reference table identifying the codes classifying the facility according to its physical equipment or principal service performed.','USER','dbo','TABLE','R_FACILITY_TYPE';
go

execute sp_addextendedproperty 'Description','FACILITY CROSS REFERENCE TYPE: the type of relationship between facilities, such as a component facility comprising part of a larger facility, a facility attached to another facility etc.','USER','dbo','TABLE','R_FACILITY_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','FACILITY FUNCTION: A list of valid functions that are satisifed by a facility, such as measurement, transportation, processing, storage, seperation etc.','USER','dbo','TABLE','R_FAC_FUNCTION';
go

execute sp_addextendedproperty 'Description','FACILITY LICENSE CONDITION TYPE: the type of condition applied to the facility license, such as flaring rate, venting rate, production rate, H2S content limit, emissions etc.','USER','dbo','TABLE','R_FAC_LIC_COND';
go

execute sp_addextendedproperty 'Description','FACILITY LICENSE CONDITION CODE: A validated code associated with a type of condition applied to the facility license.','USER','dbo','TABLE','R_FAC_LIC_COND_CODE';
go

execute sp_addextendedproperty 'Description','DUE CONDITION: The state that must be achieved for the condition to become effective. For example, a report may be due 60 days after operations commence (or cease).','USER','dbo','TABLE','R_FAC_LIC_DUE_CONDITION';
go

execute sp_addextendedproperty 'Description','FACILITY LICENSE EXTENSION CONDITION: The criteria that must be addressed in order for the primary term of the license to be extended. For example, construction must be started etc.','USER','dbo','TABLE','R_FAC_LIC_EXTEND_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE VIOLATION TYPE: The type of violation of a license that is being recorded. Can be as simple as failure to submit necessary reports or something more difficult such as improper procedures.','USER','dbo','TABLE','R_FAC_LIC_VIOLATION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LICENSE VIOLATION RESOLUTION TYPE: The type of resolution to a violation of a license term, such as the payment of a fine or creation of new processes.','USER','dbo','TABLE','R_FAC_LIC_VIOL_RESOL';
go

execute sp_addextendedproperty 'Description','FACILITY MAINTENANCE TYPE: The type of maintenance that is done on a facility, such as cleaning, painting, calibrations etc.','USER','dbo','TABLE','R_FAC_MAINTAIN_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE FACILITY MAINTAIN STATUS: the status of a facility maintenace event, such as approved, started, underway, completed, inspected etc. Note that statuses are defined within the framework of a point of view, such as operational, financial etc.','USER','dbo','TABLE','R_FAC_MAINT_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE FACILITY MAINTAIN STATUS TYPE: The type or perspective of status for a facility maintenance event, such as operational, financial, legal etc.','USER','dbo','TABLE','R_FAC_MAINT_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','PIPELINE COVER TYPE: A list of valid types of material that covers or surrounds a pipeline that is buried below ground level (or sea level).','USER','dbo','TABLE','R_FAC_PIPE_COVER';
go

execute sp_addextendedproperty 'Description','PIPELINE MATERIAL: The material that a pipeline is constructed from, such as 24 pound steel etc.','USER','dbo','TABLE','R_FAC_PIPE_MATERIAL';
go

execute sp_addextendedproperty 'Description','PIPELINE TYPE: A list of valid types of pipelines.','USER','dbo','TABLE','R_FAC_PIPE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE FACILITY SPECIFICATION REFERENCE TYPE: the type of reference that a specification is measured against. For examples, a tank may store various volumes at specific pressures or temperatures.','USER','dbo','TABLE','R_FAC_SPEC_REFERENCE';
go

execute sp_addextendedproperty 'Description','REFERENCE FACILITY STATUS TYPE: A list of the types of status that may be tracked for a facility, such as construction, production, reclamation, operational, flaring etc.','USER','dbo','TABLE','R_FAC_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE FAULT TYPE: A reference table identifying the type of fault. For example normal, reverse, strike, slip, or thrust.','USER','dbo','TABLE','R_FAULT_TYPE';
go

execute sp_addextendedproperty 'Description','FIELD COMPONENT TYPE: The type of component associated with a field.','USER','dbo','TABLE','R_FIELD_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE FIELD STATION TYPE: The type of field station, such as measured section, outcrop etc.','USER','dbo','TABLE','R_FIELD_STATION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE FIELD TYPE: A reference table identifying the type of field. For example regulatory or locally assigned.','USER','dbo','TABLE','R_FIELD_TYPE';
go

execute sp_addextendedproperty 'Description','FINANCE COMPONENT TYPE: the reason why the component is associated with the AFE, such as drilling costs, processing costs, land bid costs etc.','USER','dbo','TABLE','R_FIN_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE FINANCE CENTER COST TYPE: the type of cost associated with the AFE or cost center.','USER','dbo','TABLE','R_FIN_COST_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE FINANCE STATUS: the current status of the financial reference, such as waiting for approval, closed out, active etc.','USER','dbo','TABLE','R_FIN_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE FINANCE TYPE: The type of financial reference, such as AFE, cost center, legder etc.','USER','dbo','TABLE','R_FIN_TYPE';
go

execute sp_addextendedproperty 'Description','FINANCE CROSS REFERENCE TYPE: The type of relationship between cost center numbers or AFEs. Could be subordinate, replacement or a detail AFE for example.','USER','dbo','TABLE','R_FIN_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE FLUID TYPE: A reference table identifying the type of fluids or substances produced by a well or used for various operations. For example oil, gas, mud or water. NOTE: This reference table is still being evaluated for possible subtyping.','USER','dbo','TABLE','R_FLUID_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE FONT: A list of valid fonts, such as ARIAL or TIMES NEW ROMAN. Fonts are designs that govern the types of characters and symbols that can be displayed, and the design or apperance of those displays.','USER','dbo','TABLE','R_FONT';
go

execute sp_addextendedproperty 'Description','REFERENCE FONT EFFECT: The special effect assigned to this display, such as bold, italic, normal.','USER','dbo','TABLE','R_FONT_EFFECT';
go

execute sp_addextendedproperty 'Description','REFERENCE FOOTAGE ORIGIN: A reference table that identifies the valid points of origin used in measuring the survey footage calls to a well location.','USER','dbo','TABLE','R_FOOTAGE_ORIGIN';
go

execute sp_addextendedproperty 'Description','FOSSILTAXON LEAF NAME ALIAS REASO or TYPE: The type of taxon leaf alias name that has been created.','USER','dbo','TABLE','R_FOS_ALIAS_TYPE';
go

execute sp_addextendedproperty 'Description','FOSSIL ASSEMBLAGE TYPE: a type of fossil assemblage, such as formal, zonal, working, informal etc.','USER','dbo','TABLE','R_FOS_ASSEMBLAGE_TYPE';
go

execute sp_addextendedproperty 'Description','FOSSIL DESCRIPTION CODE: A list of valid code values for each type of fossil description type. Note the two part primary key allows each description type to have its own specific lists of code values.','USER','dbo','TABLE','R_FOS_DESC_CODE';
go

execute sp_addextendedproperty 'Description','FOSSIL DESCRIPTION TYPE: A list of valid description types for fossils. May include descriptors such as color, size, spines, shape, composition etc.','USER','dbo','TABLE','R_FOS_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','FOSSIL LIFE HABIT: The life habit of the fossil, or where it typically is found during life, such as benthic, planctonic etc.','USER','dbo','TABLE','R_FOS_LIFE_HABIT';
go

execute sp_addextendedproperty 'Description','FOSSIL NAME SET TYPE: The type of fossil name set, such as MMS, GSC, working or archival.','USER','dbo','TABLE','R_FOS_NAME_SET_TYPE';
go

execute sp_addextendedproperty 'Description','FOSSIL OBSERVATION TYPE: The type of observation that is recorded, such as lithology, structure, fossil condition etc.','USER','dbo','TABLE','R_FOS_OBS_TYPE';
go

execute sp_addextendedproperty 'Description','FOSSILTAXONOMIC GROUP: The taxonomic group that has been assigned to a fossil such as ostracod, diatom, foraminifera etc.','USER','dbo','TABLE','R_FOS_TAXON_GROUP';
go

execute sp_addextendedproperty 'Description','FOSSILTAXONOMIC LEVEL: The level of the taxonomic hierarchy at which this leaf has been identified, such as species, sub species, genus, sub genus etc.','USER','dbo','TABLE','R_FOS_TAXON_LEVEL';
go

execute sp_addextendedproperty 'Description','FOSSILTAXONOMIC GROUP: The taxonomic group that has been assigned to a fossil. Fossils may belong to a genus, subgenus, species or subspecies.','USER','dbo','TABLE','R_FOS_XREF';
go

execute sp_addextendedproperty 'Description','GAS ANALYSIS VALUE CODE: The code assigned to the analysis property by observation, in cases where numeric values are not used.','USER','dbo','TABLE','R_GAS_ANL_VALUE_CODE';
go

execute sp_addextendedproperty 'Description','GAS ANALYSIS VALUE TYPE: This table is used to the type of text values for the gas chromatography.','USER','dbo','TABLE','R_GAS_ANL_VALUE_TYPE';
go

execute sp_addextendedproperty 'Description','GRANTED RIGHT TYPE: The type of right granted to the holder. May include title, lease, P and NG lease, license, Permit P and NG, SDL, SDA, Exploration license, production license, drilling license, JOA, Pooling agreement etc. Called Document type by some systems.','USER','dbo','TABLE','R_GRANTED_RIGHT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE HEAT CONTENT METHOD: stores the types of methods used to measure or calculated the heat content of a gas sample.','USER','dbo','TABLE','R_HEAT_CONTENT_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE HOLE CONDITION: A reference table describing the condition of the wellbore. For example washed-out, sluffed or mud cake.','USER','dbo','TABLE','R_HOLE_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE HORIZONTAL DRILLING REASON CODE: This reference table identifies the reason for drilling a horizontal well. For example, some of the reasons for drilling a horizontal well are: Water coning, Intersecting a fracture system or Incr easing productivity.','USER','dbo','TABLE','R_HORIZ_DRILL_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE HORIZONTAL DRILLING TYPE: This reference table defines the type of horizontal drilling. For example, Steered-bottom hole assembly or Non-steered.','USER','dbo','TABLE','R_HORIZ_DRILL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE INCIDENT COMPONENT ROLE: Use this table to keep track of the role that an object plays in an HSE incident.','USER','dbo','TABLE','R_HSE_COMP_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE INCIDENT COMPONENT TYPE: Use this column to describe the type of component associated with the incident, such as well, building, facility etc. Use the foreign keys to create associations to the specific objects.','USER','dbo','TABLE','R_HSE_INCIDENT_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE INCIDENT DETAIL: The details about the incident, such as specific things that happened. Each thing that happened should be tracked at the level necessary for reporting and analysis.','USER','dbo','TABLE','R_HSE_INCIDENT_DETAIL';
go

execute sp_addextendedproperty 'Description','REFERENCE INCIDENT ACTION RESPONSE TYPE: A valid type of action taken in response to an incident, such as evacuation, called air ambulance, shut down, apply first aid etc.','USER','dbo','TABLE','R_HSE_RESPONSE_TYPE';
go

execute sp_addextendedproperty 'Description','R IMAGE CALIBRATION METHOD: The method used to calibrate an image, such as manual, interpolation, scale detection etc.','USER','dbo','TABLE','R_IMAGE_CALIBRATE_METHOD';
go

execute sp_addextendedproperty 'Description','R IMAGE SECTION TYPE: The type of section on an image, such as header, tool configuration, well diagram, upper scale, lower scale, section, repeat pass, high resolution etc.','USER','dbo','TABLE','R_IMAGE_SECTION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE INCIDENT BA ROLE: The role or function of a party in an incident, such as victim, medic, safety officer etc.','USER','dbo','TABLE','R_INCIDENT_BA_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE INCIDENT CAUSE CODE: a code that refines the general cause of an incident.','USER','dbo','TABLE','R_INCIDENT_CAUSE_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE INCIDENT CAUSE TYPE: A list of valid causes of an event, such as negligence, equipment failure, act of God, Act of Terrorism, vandalism or human error.','USER','dbo','TABLE','R_INCIDENT_CAUSE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE HSE INCIDENT INTERACTION TYPE: the type of interaction among components of an incident.','USER','dbo','TABLE','R_INCIDENT_INTERACT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE INCIDENT RESPONSE RESULT: The result of the action taken, where applicable. May be used to indicate what actions are successful and have the desired result.','USER','dbo','TABLE','R_INCIDENT_RESP_RESULT';
go

execute sp_addextendedproperty 'Description','REFERENCE HSE INCIDENT SUBSTANCE: Identifies any substance involved with an HSE incident. This may be a hydrocarbon, a drilling fluid, fire retardent etc.','USER','dbo','TABLE','R_INCIDENT_SUBSTANCE';
go

execute sp_addextendedproperty 'Description','REFERENCE HSE INCIDENT SUBSTANCE ROLE: Identifies the role played by a substance in an HSE Incident. Could be a spilled substance, used as fire retardant or used in first aid.','USER','dbo','TABLE','R_INCIDENT_SUBST_ROLE';
go

execute sp_addextendedproperty 'Description','Describes the technical transformation process used to generate one te chical item from another. For seismic trace data, this may be the app lication of flattening or migration algorithms. For survey data, this may be the computation of raw survey notes.','USER','dbo','TABLE','R_INFORMATION_PROCESS';
go

execute sp_addextendedproperty 'Description','REFERENCE INPUT TYPE: The type of input into an electrical device. Usually measured in Watts.','USER','dbo','TABLE','R_INPUT_TYPE';
go

execute sp_addextendedproperty 'Description','INSPECTION COMPONENT TYPE: The type of component that is associated. Can be a broker (if it is a Business Associate) or the inspected document (if it is an information item) or an inspected line ( if it is a seismic set).','USER','dbo','TABLE','R_INSP_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','INSPECTION STATUS: The status of this inspection, such as completed, waiting on approval, waiting for client decision etc.','USER','dbo','TABLE','R_INSP_STATUS';
go

execute sp_addextendedproperty 'Description','INSTRUMENT COMPONENT TYPE: The type of component associated with an instrument','USER','dbo','TABLE','R_INSTRUMENT_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','R LAND INSTRUMNT TYPE: may be caveat, Cert of non dev, assignment, mortgage, discharge etc','USER','dbo','TABLE','R_INSTRUMENT_TYPE';
go

execute sp_addextendedproperty 'Description','INSTRUMENT DETAIL CODE: In the case that the instrument detail is described as a coded value, this table provides the list of valid codes for each type of detail.','USER','dbo','TABLE','R_INST_DETAIL_CODE';
go

execute sp_addextendedproperty 'Description','INSTRUMENT DETAIL REFERENCE VALUE TYPE: In the case where a detail is referenced to some other value (such as a submission due after a certain period, or a date or an activity), the type of reference value is captured here. The values, if relevant, arestored in associated columns.','USER','dbo','TABLE','R_INST_DETAIL_REF_VALUE';
go

execute sp_addextendedproperty 'Description','INSTRUMENT DETAIL TYPE: The kind of detail information about the instrument that has been stored.','USER','dbo','TABLE','R_INST_DETAIL_TYPE';
go

execute sp_addextendedproperty 'Description','ORIGIN TYPE: The type of originating source of the interpretation. This could be a tape or disk stored in the Records module or an intermediate or final output product from a processing flow. The latter are best used in interpretation systems where the interpreted product may be ephemeral or stored only within the processing system.','USER','dbo','TABLE','R_INTERP_ORIGIN_TYPE';
go

execute sp_addextendedproperty 'Description','INTERPRETATION TYPE: the type of interpretation, such as time, depth, amplitude, shifts etc.','USER','dbo','TABLE','R_INTERP_TYPE';
go

execute sp_addextendedproperty 'Description','INTEREST SET COMPONENT: THe type of component that belong to the interest set, such as wells, land rights or wrongs, contracts and obligations.','USER','dbo','TABLE','R_INT_SET_COMPONENT';
go

execute sp_addextendedproperty 'Description','REFERENCE BA INTEREST SET ROLE: the role played by a partner in the interest set, such as operator.','USER','dbo','TABLE','R_INT_SET_ROLE';
go

execute sp_addextendedproperty 'Description','R INTEREST SET STATUS: The status of a partnership, from a planning and approval perspective or an operational perspective. The status of the partnership from various perspectives (legal, finance, operations, land managers etc) may be tracked.','USER','dbo','TABLE','R_INT_SET_STATUS';
go

execute sp_addextendedproperty 'Description','R INTEREST SET STATUS TYPE: Identifies the perspective from which the status is measured, such as financial, operational, legal, regulatory etc.','USER','dbo','TABLE','R_INT_SET_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE BA INTEREST SET TRIGGER: the event that triggered a change in the interest set shares or roles. When the event occurs, a new row in INTEREST SET is created using a new SEQUENCE NUMBER to identify the new version of the interest set.','USER','dbo','TABLE','R_INT_SET_TRIGGER';
go

execute sp_addextendedproperty 'Description','REFERENCE BA INTEREST SET TYPE: the type of interest set, such as working, royalty etc.','USER','dbo','TABLE','R_INT_SET_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE BA INTEREST SET CROSS REFERENCE TYPE: The type of relationship between interest sets. Interest sets may supercede each other, have an impact on the net worth of the interest set etc.','USER','dbo','TABLE','R_INT_SET_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE INVENTORY MATERIAL TYPE: The type of material that is tracked in a general sense. Specific kinds of equipment should be tracked in CAT EQUIPMENT and specific kinds of additives should be tracked in CAT ADDITIVE.','USER','dbo','TABLE','R_INV_MATERIAL_TYPE';
go

execute sp_addextendedproperty 'Description','INFORMATION ITEM CATEGORY: the category of information item, such as May be Acquisition support products, trace products etc.','USER','dbo','TABLE','R_ITEM_CATEGORY';
go

execute sp_addextendedproperty 'Description','ITEM SUB CATEGORY: THE SUB CATEGORY OF INFORMATION OR PHYSICAL ITEM. MAY BE ACQUISITION SUPPORT DATA ETC.','USER','dbo','TABLE','R_ITEM_SUB_CATEGORY';
go

execute sp_addextendedproperty 'Description','LAND ACQUISITION MEHOD: the method used to acquire the rights to this land right. May be purchase, lease, license, partnership, farmin, farmout, rental etc.','USER','dbo','TABLE','R_LAND_ACQTN_METHOD';
go

execute sp_addextendedproperty 'Description','LAND AGREEMENT TYPE: The type of land agreement, can be an additional qualification of LAND RIGHT.GRANTED RIGHT TYPE, for more descriptive details about the type of granted right.','USER','dbo','TABLE','R_LAND_AGREE_TYPE';
go

execute sp_addextendedproperty 'Description','R LAND BIDDER TYPE: broker, self, partner.','USER','dbo','TABLE','R_LAND_BIDDER_TYPE';
go

execute sp_addextendedproperty 'Description','R LAND BID STATUS: pending, successful, unsucessful etc.','USER','dbo','TABLE','R_LAND_BID_STATUS';
go

execute sp_addextendedproperty 'Description','R LAND BID TYPE: May be sliding scale, grouped, straight.','USER','dbo','TABLE','R_LAND_BID_TYPE';
go

execute sp_addextendedproperty 'Description','R LAND CASE ACTION: last action made to the case file.','USER','dbo','TABLE','R_LAND_CASE_ACTION';
go

execute sp_addextendedproperty 'Description','R LAND CASE TYPE: timber, geothermal....','USER','dbo','TABLE','R_LAND_CASE_TYPE';
go

execute sp_addextendedproperty 'Description','CASH BID TYPE: The type of cash bid. This is used to determine the method used to process the complete bid. May be a sliding scale bid, group bid... In a sliding scale bid, bids are placed on parcels in order of importance - if the first priority bidis accepted, the second bid may or may not be considered (depending on whether the bid is contingent on acceptance). In a grouped bid, all parcels with the same priority must be accepted or rejected together. Not to be used for Work bids.','USER','dbo','TABLE','R_LAND_CASH_BID_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LAND COMPONENT TYPE: the type of component that is associated with the land right, such as a contract or a facility.','USER','dbo','TABLE','R_LAND_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','R LAND LESSOR TYPE: the type of lessor, such as federal, indian, publ ic, BIA, Aboriginal','USER','dbo','TABLE','R_LAND_LESSOR_TYPE';
go

execute sp_addextendedproperty 'Description','R LAND OFFRING STATUS: postponed, cancelled, withdrawn, active, sold, not sold','USER','dbo','TABLE','R_LAND_OFFRING_STATUS';
go

execute sp_addextendedproperty 'Description','R LAND OFFERING TYPE: state, indian, federal, BLM, first nations, provincial, OCS, crown','USER','dbo','TABLE','R_LAND_OFFRING_TYPE';
go

execute sp_addextendedproperty 'Description','R LAND PROPERTY TYPE: the property designation for reporting acreages, such as core, non core, core developed, core non developed e tc.','USER','dbo','TABLE','R_LAND_PROPERTY_TYPE';
go

execute sp_addextendedproperty 'Description','R LAND REF NUM TYPE: The type of reference number, such as previous title number, government number etc.','USER','dbo','TABLE','R_LAND_REF_NUM_TYPE';
go

execute sp_addextendedproperty 'Description','R LAND RENTAL TYPE: A delay rental is made to defer requirement to drill during the primary term of a lease. An annual rental is made in addition to any subsequent royalty payment due to production. Shut in royalty is made in lieu of the royalty payment and is usually equivalent to the delay or rental amount, or can be on a well by well basis.','USER','dbo','TABLE','R_LAND_RENTAL_TYPE';
go

execute sp_addextendedproperty 'Description','R LAND REQUEST TYPE: The type of request that was made, such as a Call for Nominations or a Posting Request. Typically, a Call for Nominations is created by a regulatory agency (in Canada, this is done by Yukon, Nortwest Territory and Nunuvit). Industry responds with posting requests, usually the company that creates a posting request is obligated to bid on the resultant land sale offering.','USER','dbo','TABLE','R_LAND_REQUEST_TYPE';
go

execute sp_addextendedproperty 'Description','R LAND REQUEST STATUS: pending, refused, accepted','USER','dbo','TABLE','R_LAND_REQ_STATUS';
go

execute sp_addextendedproperty 'Description','R LAND RIGHT CATEGORY: The category of land right. May be Mineral or Surface','USER','dbo','TABLE','R_LAND_RIGHT_CATEGORY';
go

execute sp_addextendedproperty 'Description','R LAND RIGHT STATUS: continued, held by production, termination, inactivated, surrendered.','USER','dbo','TABLE','R_LAND_RIGHT_STATUS';
go

execute sp_addextendedproperty 'Description','LAND STATUS TYPE: the type of status for a land right, such as legal, financial or working.','USER','dbo','TABLE','R_LAND_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','R LAND TITLE CHANGE REASON: seperation, consolodation, transfer of la nd or interest','USER','dbo','TABLE','R_LAND_TITLE_CHG_RSN';
go

execute sp_addextendedproperty 'Description','REFERENCE LAND TITLE TYPE: the type of land title held.','USER','dbo','TABLE','R_LAND_TITLE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LAND UNIT TRACT TYPE: the type of land unit tract.','USER','dbo','TABLE','R_LAND_TRACT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LAND UNIT TYPE: the type of land unit.','USER','dbo','TABLE','R_LAND_UNIT_TYPE';
go

execute sp_addextendedproperty 'Description','R LAND WELL RELATIONSHIP TYPE: the type of relationship between the well and the land right. For example, a well may be inferred to be related to a land right because of its location, or the relationship may be explicitly stated in an agreement. In some cases, a well may not be located physically in a land right in order to have an association.','USER','dbo','TABLE','R_LAND_WELL_REL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LANGUAGE: The form of a means of communicating ideas or feelings by the use of conventionalized signs, sounds, gestures, or marks having understood meanings. Usually the language used in a document, proceding, process or appllication.','USER','dbo','TABLE','R_LANGUAGE';
go

execute sp_addextendedproperty 'Description','REFERENCE LEASE UNIT STATUS: A reference table identifying the operational or legal status of the production lease or unit.','USER','dbo','TABLE','R_LEASE_UNIT_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE LEASE UNIT TYPE: A reference table identifying the type of production lease or unit. For example, Federal Lease, State Lease , Indian Lease, Production Unit, etc.','USER','dbo','TABLE','R_LEASE_UNIT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LEGAL SURVEY TYPE: A reference table identifying valid survey types used for legal descriptions. For example, Carter, Congressional, Dominion Land Survey, ...','USER','dbo','TABLE','R_LEGAL_SURVEY_TYPE';
go

execute sp_addextendedproperty 'Description','LICENSE STATUS: the status of the license, such as pending, approved, terminated, cancelled by operator, denied, extended etc.','USER','dbo','TABLE','R_LICENSE_STATUS';
go

execute sp_addextendedproperty 'Description','LICENSE STATUS TYPE: the type of status described for the license. Could be working, file, activity, regulatory, environmental etc. Used to track the situation where multiple types of statuses are to be tracked.','USER','dbo','TABLE','R_LIC_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LINER TYPE: This reference table describes the type of liner used in the borehole. For example, slotted, gravel packed or pre-perforated etc.','USER','dbo','TABLE','R_LINER_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGY: This reference table describes the major lithologic types. For example sandstone, limestone, dolomite or shale.','USER','dbo','TABLE','R_LITHOLOGY';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGIC ABUNDANCE: Relative abundance of each color rank (first, second, third:1, 2, 3) or as a ranked magnitude (abundant, common, rare). Used in the litholgy model.','USER','dbo','TABLE','R_LITH_ABUNDANCE';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGIC BOUNDARY TYPE: Type of boundary occurring between two adjacent rock intervals (e.g., unconformable, nonconformable, conformable, etc.).','USER','dbo','TABLE','R_LITH_BOUNDARY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGIC COLOR: Basic color or color adjective of lithologic components such as red, grey, blue etc. Used in Litholgoy','USER','dbo','TABLE','R_LITH_COLOR';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGIC CONSOLIDATION: Consolidation or induration of the rock sample. Induration of a rock sample (sandstone) refers to its resistance to physical disaggregation and does not necessarily refer to hardness of the constituent grains. Common types of consolidation (induration) are, dense, hard, medium hard, soft, spongy or friable.','USER','dbo','TABLE','R_LITH_CONSOLIDATION';
go

execute sp_addextendedproperty 'Description','REFERENCE CYCLE BED: a sequence of beds or related processes and conditions, repeated in the same order that is recorded in a sedimentary deposit.','USER','dbo','TABLE','R_LITH_CYCLE_BED';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGIC DEPOSITIONAL ENVIRONMENT: Type of interpreted environment of the deposition. A depositional environment is the physical environment in which sediments were deposited. For example, a high-energy river channel or a carbonate barrier reef.','USER','dbo','TABLE','R_LITH_DEP_ENV';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGIC DIAGENESIS TYPE: Type of diagenesis or diagenetic mineral which is found in the described interval. Common types of diagenesis are compaction, cementation, recrystallization or dolomitization. Diagenetic minerals may include dolomite, glauconite, olivine, etc.','USER','dbo','TABLE','R_LITH_DIAGENESIS';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGIC DISTRIBUTION :Describes the distribution of the rock color in the sub-interval. For example, the color distribution maybe graded, uneven, etc.','USER','dbo','TABLE','R_LITH_DISTRIBUTION';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGIC INTENSITY: Rock color intensity. The color intensity is used to describe the amount of color associated with the sample. For example, the color intensity may range from high to low.','USER','dbo','TABLE','R_LITH_INTENSITY';
go

execute sp_addextendedproperty 'Description','LITHOLOGY LOG COMPONENT TYPE: The type of component associated with a lithological log','USER','dbo','TABLE','R_LITH_LOG_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGIC LOG TYPE: The type of log may be interpretive, percentage, qualified percentage, composite interpretive or descriptive.','USER','dbo','TABLE','R_LITH_LOG_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE OIL STAIN: the type of oil stain observed in the rock sample. For example, the oil stain can be described as fair live oil, questionable, etc','USER','dbo','TABLE','R_LITH_OIL_STAIN';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGIC PATTERN CODE:  A valid code for the lithologic pattern, often as noted by standard usage.','USER','dbo','TABLE','R_LITH_PATTERN_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGIC ROCKPART: Name of component such as glauconite (rock) or crinoids (fossil)','USER','dbo','TABLE','R_LITH_ROCKPART';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGIC ROCK MATRIX: Type of fine grain material filling voids between larger grains. The grained particles in a poorly sorted sedimentary rock. Matrix can be fine grained sandstone, siltstone, shale, etc.','USER','dbo','TABLE','R_LITH_ROCK_MATRIX';
go

execute sp_addextendedproperty 'Description','REFERENCE ROCK PROFILE: Identifies the type of rock weathering or borehole profile associated with the particular described interval. Examples of the rock profiles can be recessive, cliff, etc.','USER','dbo','TABLE','R_LITH_ROCK_PROFILE';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGIC ROCK TYPE: Type of rock comprising a significant portion of the interval. For example, the predominant rock type in the interval may be coal, or oolitic limestone or calcareous sandstone.','USER','dbo','TABLE','R_LITH_ROCK_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGIC ROUNDING: Classifies the shape of the rock components. This is typically used in describing clastic sedimentary rocks. There are an almost infinite number of variations in the shapes of grain size, but the most common classes are sharp, angular, subangular, rounded or globular.','USER','dbo','TABLE','R_LITH_ROUNDING';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGIC SCALE SCHEME: Type of scaling system source used to determine the grain size like the widely accepted Wentworth scale, or a company internal grade scale (Canstrat, Shell).','USER','dbo','TABLE','R_LITH_SCALE_SCHEME';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGIC SORTING: Identifies the type of sorting associated with the principal rock being described. This feature is most important in coarse clastic rocks and common examples are well, medium or poorly sorted.','USER','dbo','TABLE','R_LITH_SORTING';
go

execute sp_addextendedproperty 'Description','REFERENCE LITHOLOGIC STRUCTURE: Type of sedimentary or other rock structure occurring in the lithologic interval being described (e.g., cross-stratification, mud cracks, ripple marks, etc.). The rock structure may be non-sedimentary, such as contorted bedding or fault zone.','USER','dbo','TABLE','R_LITH_STRUCTURE';
go

execute sp_addextendedproperty 'Description','REFERENCE LOCATION DESCRIPTION TYPE: A reference table identifying valid types of location descriptions.','USER','dbo','TABLE','R_LOCATION_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LOCATION QUALIFIER: A reference table identifying valid types of locations. For example, Actual, Theorectical, Original, Contract, ... (but not Rationalized).','USER','dbo','TABLE','R_LOCATION_QUALIFIER';
go

execute sp_addextendedproperty 'Description','REFERENCE LOCATION QUALITY: Indicates the quality or degree of reliability of a location.','USER','dbo','TABLE','R_LOCATION_QUALITY';
go

execute sp_addextendedproperty 'Description','REFERENCE LOCATION TYPE: A reference table identifying the type of obj ect being given a legal location. For example, node, well, ...','USER','dbo','TABLE','R_LOCATION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LOG ARRAY DIMENSION: the dimension of the element in the well log parameter array.','USER','dbo','TABLE','R_LOG_ARRAY_DIMENSION';
go

execute sp_addextendedproperty 'Description','REFERENCE LOG DIGIT CORRECTION METHOD: This reference table identifies the method used to correct the log depth.','USER','dbo','TABLE','R_LOG_CORRECT_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE LOG CURVE CLASSIFICATION SYSTEM: The system used to generate this well log curve classification. Several systems are typically used. Value to the Customer system was created by SLB and is used by several logging companies. Other systems for classifications have been defined by logging contractors, the PWLS or E and P companies.','USER','dbo','TABLE','R_LOG_CRV_CLASS_SYSTEM';
go

execute sp_addextendedproperty 'Description','LOG DEPTH TYPE: the type of depth measurements provided in the log, such as Measured (MD) or True Vertical (TVD)','USER','dbo','TABLE','R_LOG_DEPTH_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LOG DIRECTION: The direction that the tool string was moving when the logging occured, usually UP or DOWN.','USER','dbo','TABLE','R_LOG_DIRECTION';
go

execute sp_addextendedproperty 'Description','REFERENCE LOG GOOD VALUE TYPE: A list of valid types of good values that are used to indicate the top and base of useful data gathered during logging operations.','USER','dbo','TABLE','R_LOG_GOOD_VALUE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LOG INDEX TYPE: The type of measurement index for the log, such as depth or time.','USER','dbo','TABLE','R_LOG_INDEX_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LOG MATRIX LITHOLOGY SETTING: This reference table identifies the type of lithologic material present in the rock being evaluated. For example, sandstone, limestone.','USER','dbo','TABLE','R_LOG_MATRIX';
go

execute sp_addextendedproperty 'Description','R LOG IMAGE POSITION TYPE: The type of position that is on the log section, such as top header, bottom header, left depth track, depth calibration, skew correction.','USER','dbo','TABLE','R_LOG_POSITION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE TOOL TYPE: This reference table defines the logging tools which compose a log tool string. For example, the type of wireline tool may be a compensated neutron tool, sonic tool etc.','USER','dbo','TABLE','R_LOG_TOOL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE LOST MATERIAL TYPE: A reference table describing the type of material used in treating a lost circulation interval. For example cane, chicken feather, hay or walnut hulls.','USER','dbo','TABLE','R_LOST_MATERIAL_TYPE';
go

execute sp_addextendedproperty 'Description','LAND RIGHT FACILITY CROSS REFERENCE TYPE: the type of relationship between the land right and the facility, such as produciton, operated by etc.','USER','dbo','TABLE','R_LR_FACILITY_XREF';
go

execute sp_addextendedproperty 'Description','LAND RIGHT FIELDCROSS REFERENCE TYPE: the type of relationship between the land right and the field, such as produciton, operated by etc.','USER','dbo','TABLE','R_LR_FIELD_XREF';
go

execute sp_addextendedproperty 'Description','REFERENCE LAND RIGHT SIZE TYPE: the type of size referenced, usually based on interest type, or to distinguish between onshore and offshore holdings.','USER','dbo','TABLE','R_LR_SIZE_TYPE';
go

execute sp_addextendedproperty 'Description','R LAND RIGHTTERMINATION REQUIRMENTS: a valid list of requirments for the termination of agreenents','USER','dbo','TABLE','R_LR_TERMIN_REQMT';
go

execute sp_addextendedproperty 'Description','R LR TERMIN TYPE: may be expiry, surrendor, trade, sale cancellation','USER','dbo','TABLE','R_LR_TERMIN_TYPE';
go

execute sp_addextendedproperty 'Description','R LR XREF TYPE: may be history , overlap, chain of title, mineral to c of T, lease to liscence etc','USER','dbo','TABLE','R_LR_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','R LAND OFFER CANCEL REASON: reason why the land sale offering was removed from the land sale, such as withdrawn, no bids, no acceptable bids.','USER','dbo','TABLE','R_L_OFFR_CANCEL_RSN';
go

execute sp_addextendedproperty 'Description','REFERENCE MACERAL AMOUNT: a description of the amount of maceral (trace, abundant). This is often a name that relates to a range of values, such as rare = <.1%. This is always going to be liptinite. Do not use ORGANIC MATTER TYPE (function is unclear).If used in petrology table, the meaning would need to be organic matter in coal or organic matter that is dispersedthrough the rocks or both. (Coal vs DOM dispersed organic matter - vs both). Check the Petrology table to be sure we do properly.','USER','dbo','TABLE','R_MACERAL_AMOUNT_TYPE';
go

execute sp_addextendedproperty 'Description','MAINTENANCE PROCESS: The maintenance process used, such as tape rewind and tightening.','USER','dbo','TABLE','R_MAINT_PROCESS';
go

execute sp_addextendedproperty 'Description','REFERENCE MATURATION TYPE: indicates the level of maturity of a source rock or extracted organic material. May be immature, mature or over mature','USER','dbo','TABLE','R_MATURATION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE MATURITY METHOD: This table is used to capture the type of method of maturation.','USER','dbo','TABLE','R_MATURITY_METHOD';
go

execute sp_addextendedproperty 'Description','COMPRESSIBILITY FACTORE METHOD: the method used to determine the compresibility factor.','USER','dbo','TABLE','R_MBAL_COMPRESS_TYPE';
go

execute sp_addextendedproperty 'Description','CURVE FIT TYPE: the type of curve, such as manual fit, best fit, weighted best fit.','USER','dbo','TABLE','R_MBAL_CURVE_TYPE';
go

execute sp_addextendedproperty 'Description','MEASUREMENT LOCATION TYPE: The type of location the measurment was taken from. For example, center of the core, sidewall, etc.','USER','dbo','TABLE','R_MEASUREMENT_LOC';
go

execute sp_addextendedproperty 'Description','REFERENCE MEASUREMENT TYPE: A reference table identifying the type of measurement recorded.','USER','dbo','TABLE','R_MEASUREMENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE MEASURE TECHNIQUE: This reference table describes the various flow measurement techniques used for well tests. For example orifice meter, separator or estimated.','USER','dbo','TABLE','R_MEASURE_TECHNIQUE';
go

execute sp_addextendedproperty 'Description','MEDIA TYPE: Describes the type of media used for the physical rendering of an item. Allowable types include 8 mm tape, 9 inch tape, backup unet, cassette, diskette, epoch, film, linen, microfilm, mylar, negative, original, paper, photo print, print, reproducable, sepia, xerox, worm optical disk, unknown, mixed.','USER','dbo','TABLE','R_MEDIA_TYPE';
go

execute sp_addextendedproperty 'Description','METHOD TYPE: The type of method being used in the sample analysis.','USER','dbo','TABLE','R_METHOD_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE MISCELLANEOUS DATA CODE: A coded value associated with a miscellaneous data type, in the case where the value is selected from a list of valid values.','USER','dbo','TABLE','R_MISC_DATA_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE MISCELLANEOUS DATA TYPE: A reference table identifying the type of miscellaneous or generic data associated with an entity and stored at a general level. Examples: culture, safety, financial, regulatory, environment.','USER','dbo','TABLE','R_MISC_DATA_TYPE';
go

execute sp_addextendedproperty 'Description','MISSING_STRAT_TYPE:. The reason why a portion of a strat unit is missing. Examples: fault, unconformity, fold.','USER','dbo','TABLE','R_MISSING_STRAT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE MOBILITY TYPE: The type of color the mobility marker used in the fluorescence analysis is. The mobility marker can be used to track any movement of the cells it is attached to or the marker can be used to visualize stable dissolution of different substances.','USER','dbo','TABLE','R_MOBILITY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE MONTH: A reference table identifying a valid month. For example January, February etc.','USER','dbo','TABLE','R_MONTH';
go

execute sp_addextendedproperty 'Description','REFERENCE MUD COLLECTION REASON: the reason or business process behind the collection of the mud sample, such as logging or drilling.','USER','dbo','TABLE','R_MUD_COLLECT_REASON';
go

execute sp_addextendedproperty 'Description','R MUD LOG COLOR: Observed colors resulting from llithologic analysis, such as fluorescence_color (Color of hydrocarbon as viewed in ultraviolet light.) cut_color (Color of hydrocarbon extracted by reagent/solvent in ultraviolet light.','USER','dbo','TABLE','R_MUD_LOG_COLOR';
go

execute sp_addextendedproperty 'Description','REFERENCE MUD PROPERTY CODE: A validated code for the property being measured, in the case where the response is selected from a list of allowed values. In each case, the codes allowed are specific to the property type being measured.','USER','dbo','TABLE','R_MUD_PROPERTY_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE MUD PROPERTY REFERENCE TYPE: The type of mud property reference that qualifies a mud property measurement, such as temperature.','USER','dbo','TABLE','R_MUD_PROPERTY_REF';
go

execute sp_addextendedproperty 'Description','REFERENCE MUD PROPERTY TYPE: The type of mud property being measured, such as alkalinity. Various properties may be measured using numbers, codes or textual descriptions.','USER','dbo','TABLE','R_MUD_PROPERTY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE MUD SAMPLE TYPE: This reference table identifies the type of mud sample used for determining mud resistivity. For example mud cake, filtrate or mud.','USER','dbo','TABLE','R_MUD_SAMPLE_TYPE';
go

execute sp_addextendedproperty 'Description','R MUNICIPALITY: name of the municipality','USER','dbo','TABLE','R_MUNICIPALITY';
go

execute sp_addextendedproperty 'Description','REFERENCE STRAT NAME SET CROSS REFERENCE TYPE: the type of cross reference that exists between name sets. May be used to indicate that one name set has replaced another, that a name set is a subset of another, that strat unit names in a name set are automatically converted to another for data loads etc.','USER','dbo','TABLE','R_NAME_SET_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE NODE POSITION: A reference table identifying valid positions on a wellbore path. For example, surface point, bottomhole point, or kick-off point.','USER','dbo','TABLE','R_NODE_POSITION';
go

execute sp_addextendedproperty 'Description','REFERENCE NORTH: A reference table identifying valid north references used in surveying to define angular measurements. For example, True North, Magnetic North, Grid North, Astronomical North, ...','USER','dbo','TABLE','R_NORTH';
go

execute sp_addextendedproperty 'Description','NOTIFICATION COMPONENT TYPE: The type of component associated with a notification','USER','dbo','TABLE','R_NOTIFICATION_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE NOTIFICATION TYPE: the type of notification.','USER','dbo','TABLE','R_NOTIFICATION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE NORTH-SOUTH DIRECTION: A reference table identifying valid north-south Directions. For example, North, South.','USER','dbo','TABLE','R_NS_DIRECTION';
go

execute sp_addextendedproperty 'Description','REFERENCE NORTH-SOUTH START LINE: A reference table identifying valid north-south starting lines for offset distances. This is used primarily for non-orthonormal survey blocks, such as Texas surveys and California blocks. For example, FS L first south line, ESL eastmost south line, ...','USER','dbo','TABLE','R_NS_START_LINE';
go

execute sp_addextendedproperty 'Description','REFERENCE OBLIGATION CALCULATION METHOD: the method used to calculate the obligation.','USER','dbo','TABLE','R_OBLIG_CALC_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE OBLIGATION CALCULATION POINT OF DEDUCTION: the point at which the calculation is taken, such as at the wellhead. May be for deduction or obligation calculation.','USER','dbo','TABLE','R_OBLIG_CALC_POINT';
go

execute sp_addextendedproperty 'Description','R OBLIG CATEGORY: may be one of non-payment obligation, rental, lease or royalty','USER','dbo','TABLE','R_OBLIG_CATEGORY';
go

execute sp_addextendedproperty 'Description','OBLIGATION COMPONENT TYPE: The type of component associated with an obligation','USER','dbo','TABLE','R_OBLIG_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','OBLIGATION COMPONENT REASON: the reason why the component is associated with the obligation. For example, seismic data may be acquired to satisfy the terms of an obligation, a contract may govern the management of an obligation or a document may provide a record of the obligation.','USER','dbo','TABLE','R_OBLIG_COMP_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE OBLIGATION DEDUCTION CALCULATION METHOD: the method used to calculate the deduction, such as the MIN/MAX method','USER','dbo','TABLE','R_OBLIG_DEDUCT_CALC';
go

execute sp_addextendedproperty 'Description','REFERENCE OBLIGATION PARTY TYPE: the type of party for the obligation, specifically whether he is a PAYEE or a PAYOR. Used to support identification of Burden Bearer relationships for obligations.','USER','dbo','TABLE','R_OBLIG_PARTY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE OBLIGATION PAYMENT RESPONSIBILITY: Indicates whether the obligation is paid out entirely by one partner, each partner is responsible for paying only their share, or whether a third party has been retained to make payments and charge back to each partner.','USER','dbo','TABLE','R_OBLIG_PAY_RESP';
go

execute sp_addextendedproperty 'Description','REFERENCE OBLIGATION REMARK TYPE: The type of remark, such as work obligation fulfillment, general, payment etc.','USER','dbo','TABLE','R_OBLIG_REMARK_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE OBLIGATION SUSPEND PAYMENT REASON: the reason the payment was suspended, such as bankruptcy.','USER','dbo','TABLE','R_OBLIG_SUSPEND_PAY';
go

execute sp_addextendedproperty 'Description','REFERENCE OBLIGATION TRIGGER METHOD: The means by which the obligation is triggered, and becomes active.','USER','dbo','TABLE','R_OBLIG_TRIGGER';
go

execute sp_addextendedproperty 'Description','R OBLIG TYPE: the type of obligation incurred, such as termination notice, surrendor notice, annual rental','USER','dbo','TABLE','R_OBLIG_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE OBLIGATION CROSS REFERENCE TYPE: the reason the obligations have been related to each other, such as a lease rental payment that has been allocated to its subordinate lease components.','USER','dbo','TABLE','R_OBLIG_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE OFFSHORE AREA CODE: A reference table identifying valid offshore area codes used for US offshore well locations. For example,','USER','dbo','TABLE','R_OFFSHORE_AREA_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE OFFSHORE COMPLETION TYPE: This reference table identifies the location of the completion equipment on the drilling rig. For example, values for the offshore completion can be above water, under water etc.','USER','dbo','TABLE','R_OFFSHORE_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','OIL VALUE CODE: Use this table to store the code assigned to the analysis oil by observation, in cases where numeric values are not used.','USER','dbo','TABLE','R_OIL_VALUE_CODE';
go

execute sp_addextendedproperty 'Description','ONTOGENY TYPE: The early life history of an organism, i.e., the subsequent stages it passes from the zygote to the mature adult.','USER','dbo','TABLE','R_ONTOGENY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE OPERAND QUALIFIER: This table identifies the symbols used to qualify the measurement values. For example, the symbol (>) indicates the value is more than the displayed measurement. Similarly the symbol(<) indicates the v alue is less than the displayed measurement. The symbols can include all the mathematical operands (<,>,+,=,-).','USER','dbo','TABLE','R_OPERAND_QUALIFIER';
go

execute sp_addextendedproperty 'Description','REFERENCE ORIENTATION: A reference table identifying valid orientations of measurements to reference lines. For example, parallel, perpendicular, ...','USER','dbo','TABLE','R_ORIENTATION';
go

execute sp_addextendedproperty 'Description','REFERENCE MACERAL AMOUNT: a description of the amount of maceral (trace, abundant). This is often a name that relates to a range of values, such as rare = <.1%. This is always going to be liptinite.','USER','dbo','TABLE','R_PALEO_AMOUNT_TYPE';
go

execute sp_addextendedproperty 'Description','PALEO INDICATOR TYPE: A set of indicator types typically generated during fossil analysis and interpretation. Can inlude youngest, oldest, deepest, reworked, out of place, etc.','USER','dbo','TABLE','R_PALEO_IND_TYPE';
go

execute sp_addextendedproperty 'Description','PALEO INTERPRETATION TYPE: the type of interpreted inforamtion contained, such as an age boundary, change boundary, age zone etc.','USER','dbo','TABLE','R_PALEO_INTERP_TYPE';
go

execute sp_addextendedproperty 'Description','PALEO FOSSIL TYPE: The type of type fossil identified such as holotype - first documented occurrence of this fossil in the literature, Paratype - when you add detail from other specimens, neotype - when the holotype has been lost and this is a replacement for study.','USER','dbo','TABLE','R_PALEO_TYPE_FOSSIL';
go

execute sp_addextendedproperty 'Description','PALEO SUMMARY COMPONENT TYPE: The type of component associated with a paleological summary','USER','dbo','TABLE','R_PAL_SUM_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','PALEO SUMMARY CROSS REFERENCE TYPE: the type of cross reference between paleo summaries. Could include reports that are included in another, reports that supplement or replace others or reports that are simply different versions (although we dont recommendthat you store draft copies in your database).','USER','dbo','TABLE','R_PAL_SUM_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','R PARCEL LOT TYPE: the type of parcel lot descibed. Used to describe the type of US land parcel lots, in the congressional legal survey system etc..','USER','dbo','TABLE','R_PARCEL_LOT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PARCEL TYPE: The type of land parcel. May be one of: Congressional, Carter, DLS, FPS, Geodetic, NE Loc, North sea, NTS, Offshore, Ohio, Texas.','USER','dbo','TABLE','R_PARCEL_TYPE';
go

execute sp_addextendedproperty 'Description','R PAYMENT TYPE: The type of payment that is made, such as rental, royalty. If the payment is a rental payment, the kind of rental payment is found in R LAND RENTAL TYPE. Type of royalty payments are captures in R ROYALTY TYPE.','USER','dbo','TABLE','R_PAYMENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PAYZONE TYPE: This reference table describes the type of payzone either production or pay.','USER','dbo','TABLE','R_PAYZONE_TYPE';
go

execute sp_addextendedproperty 'Description','R PAYMENT DETAIL TYPE: may be tax, bank service charge, lessor payment.','USER','dbo','TABLE','R_PAY_DETAIL_TYPE';
go

execute sp_addextendedproperty 'Description','R PAYMENT METHOD:  The method of payment made, such as direct deposit, EFT, check, money order etc.','USER','dbo','TABLE','R_PAY_METHOD';
go

execute sp_addextendedproperty 'Description','R PAYMENT RATE TYPE:  the kind of payment rate calculated, such as tax or rental','USER','dbo','TABLE','R_PAY_RATE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PDEN AMENDMENT REASON: The reason why a production amendment was posted, such as instrument calibration, calculation error or volume balancing.','USER','dbo','TABLE','R_PDEN_AMEND_REASON';
go

execute sp_addextendedproperty 'Description','PRODUCTION ENTITY COMPONENT TYPE: The type of component associated with a production entity','USER','dbo','TABLE','R_PDEN_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PDEN STATUS: A reference table identifying the state of the production entity from the point of view described in PDEN STATUS TYPE (such as operational status).','USER','dbo','TABLE','R_PDEN_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE PDEN STATUS TYPE: A reference table identifying the type of status, such as the operational status, the financial status, the legal status, the eligibility status etc.','USER','dbo','TABLE','R_PDEN_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','PRODUCTION REPORTING ENTITY CROSS REFERENCE TYPE: The type of cross reference. Used in situations where you may want two different XREF networks (ownership and physical connections like pipelines, for instance).','USER','dbo','TABLE','R_PDEN_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','PERFORATION METHOD: Code identifying the type of opening the fluid entered through into the tubing (e.g., perforation, open hole, combination, etc.).','USER','dbo','TABLE','R_PERFORATION_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE PERFORATION TYPE: A reference table identifying the type of perforation method. For example bullet, jet or combination.','USER','dbo','TABLE','R_PERFORATION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PERIOD TYPE: A reference table identifying the periods of time. Possible values include DAY, MONTH, YEAR.','USER','dbo','TABLE','R_PERIOD_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PERMEABILITY TYPE: The ability of a rock body to transmit fluids, typically measured in darcies or millidarcies. Formations that transmit fluids readily, such as sandstones, are described as permeable and tend to have many large, well-connected pores. Impermeable formations, such as shales and siltstones, tend to be finer grained or of a mixed grain size, with smaller, fewer, or less interconnected pores. This is intended to serve as a qualitative value, rather than measured values.','USER','dbo','TABLE','R_PERMEABILITY_TYPE';
go

execute sp_addextendedproperty 'Description','STATUS: may be available, lost, destroyed, unknown etc','USER','dbo','TABLE','R_PHYSICAL_ITEM_STATUS';
go

execute sp_addextendedproperty 'Description','Describes the process used to create a new physical rendering of an i tem. May be tape copy, film copy ...','USER','dbo','TABLE','R_PHYSICAL_PROCESS';
go

execute sp_addextendedproperty 'Description','REFERENCE PHYSICAL ITEM GROUP TYPE: The type of physical group created, such as a group of images that form a composite, or a montage, or a file set that are to be distributed together.','USER','dbo','TABLE','R_PHYS_ITEM_GROUP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PICK LOCATION: This reference table identifies the location or type of formation (strat unit) pick. For example top, base or middle.','USER','dbo','TABLE','R_PICK_LOCATION';
go

execute sp_addextendedproperty 'Description','REFERENCE PICK QUALIFIER: This reference table describes a qualifier used to describe a formation pick. For example not logged, missing, estimated or uncertain depth.','USER','dbo','TABLE','R_PICK_QUALIFIER';
go

execute sp_addextendedproperty 'Description','REFERENCE PICK QUALIFIER REASON: The reason that a qualifier was added for a pick, such as poor data, faulting, erosion.','USER','dbo','TABLE','R_PICK_QUALIF_REASON';
go

execute sp_addextendedproperty 'Description','REFERENCE PICK QUALITY: the quality of or degree of confidence in the pick that was made, such as poor, uncertain, good, excellent.','USER','dbo','TABLE','R_PICK_QUALITY';
go

execute sp_addextendedproperty 'Description','REFERENCE PICK VERSION TYPE: the type of version of pick that is given in an interpretation table, such as working, final, proposed etc.','USER','dbo','TABLE','R_PICK_VERSION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PLATFORM TYPE: This reference table describes a type of drilling platform or pad. For example, fixed platform, compliant tower, tension leg, or onshore pad.','USER','dbo','TABLE','R_PLATFORM_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PLOT SYMBOL: This reference table contains the required information to represent a character or plot symbol. For example this may be particular mapping package coding scheme to represent well status codes a pointer to other appl ication dependent files.','USER','dbo','TABLE','R_PLOT_SYMBOL';
go

execute sp_addextendedproperty 'Description','REFERENCE PLUG TYPE: This reference table identifies the type of operation performed to plugback the well. For example, the type of plugback may be a temporary plugback with a retrievable bridge plug or a cement plug.','USER','dbo','TABLE','R_PLUG_TYPE';
go

execute sp_addextendedproperty 'Description','POOL COMPONENT TYPE: The type of component associated with a pool','USER','dbo','TABLE','R_POOL_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE POOL STATUS: A reference table identifying the operational or legal status of the pool.','USER','dbo','TABLE','R_POOL_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE POOL TYPE: A reference table identifying the type of pool.','USER','dbo','TABLE','R_POOL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE POROSITY TYPE: This reference table describes the type of porosity observed. For example intergranular, sucrosic or cavernous.','USER','dbo','TABLE','R_POROSITY_TYPE';
go

execute sp_addextendedproperty 'Description','PPDM AUDIT REASON: The reason why an auditable change was made to the data, such as data cleanup project, new data received, incorrect data corrected, missing data located etc.','USER','dbo','TABLE','R_PPDM_AUDIT_REASON';
go

execute sp_addextendedproperty 'Description','PPDM COLUMN AUDIT TYPE: The type of change that is being tracked in this row of audit data. Depending on the business rules in place, may track inserts, updates or deletes.','USER','dbo','TABLE','R_PPDM_AUDIT_TYPE';
go

execute sp_addextendedproperty 'Description','PPDM BOOLEAN RULE:  A list of valid boolean rules, such as equals, less than, greater than etc.','USER','dbo','TABLE','R_PPDM_BOOLEAN_RULE';
go

execute sp_addextendedproperty 'Description','PPDM COLUMN KEY METHOD TYPE: The type of method that is used to create the value in this column. The method could include manual selection and key entry, automated random generation, concatenation of values etc. If you wish, the code used to generate thekey value can be stored in PPDM COLUMN, and the last assigned key can also be stored.','USER','dbo','TABLE','R_PPDM_CREATE_METHOD';
go

execute sp_addextendedproperty 'Description','PPDM COLUMN DATA TYPE: The database datatype that is assigned to a column or value. Usually, number, character, date etc.','USER','dbo','TABLE','R_PPDM_DATA_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE DEFAULT VALUE METHOD: The method used to assign a default value to this column, in the event that a default value is allowed. May include things like a SYSTEM DATE, USERID, null, or some other value. Please note that default values should be used with great caution and documentation of business rules. Default values can leave users confused, or may be misleading if they are not carefully implemented.','USER','dbo','TABLE','R_PPDM_DEFAULT_VALUE';
go

execute sp_addextendedproperty 'Description','PPDM RULE ENFORCE METHOD: The types of method that is used to enforce a rule, such as RDBMS business rule, stored procedure, function, software procedure, manual enforcement etc.','USER','dbo','TABLE','R_PPDM_ENFORCE_METHOD';
go

execute sp_addextendedproperty 'Description','PPDM RULE FAIL RESULT: the result if the enforcement of a rule fails, such as critical error, warning, notification etc.','USER','dbo','TABLE','R_PPDM_FAIL_RESULT';
go

execute sp_addextendedproperty 'Description','PPDM GROUP TYPE: the type of group that is being described. Could be an application group, query group, function group, module type etc.','USER','dbo','TABLE','R_PPDM_GROUP_TYPE';
go

execute sp_addextendedproperty 'Description','PPDM GROUP USE: The function or useage of a table or column in a group. Examples include direct usage, a dependency, a core part of the group or a referenced section etc.','USER','dbo','TABLE','R_PPDM_GROUP_USE';
go

execute sp_addextendedproperty 'Description','REFERENCE PPDM GROUP CROSS REFERENCE TYPE: The type of relationship between two groups, such as a hierarchical relationship between groups used for creating a report, replacements and other types of dependencies.','USER','dbo','TABLE','R_PPDM_GROUP_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PPDM INDEX CATEGORY: The technical category of the index, such as bit mapped or normal (regular).','USER','dbo','TABLE','R_PPDM_INDEX_CATEGORY';
go

execute sp_addextendedproperty 'Description','REFERENCE PPDM MAPPING RULE TYPE: The type of rule that is used for conditional mapping between systems, such as when the mapping depends on the value of the column, or the value of another related column.','USER','dbo','TABLE','R_PPDM_MAP_RULE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PPDM MAPPING TYPE: The type of mapping between two elements, such as data that is simply migrated from one system to the other without any change, data that is mapped through a reference table, a mapping that requires some computation or calculation or a multiple mapping where the source and target do not have a simple one to one relationship.','USER','dbo','TABLE','R_PPDM_MAP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PPDM METRIC CODE: A measurement or indicative code for a specific kind of metric. Some metrics may be measured quantitatively, through numbers, and others may be measured qualitatively, though more subjective values such as good, complete etc.','USER','dbo','TABLE','R_PPDM_METRIC_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE PPDM METRIC COMPONENT TYPE: The type of component that is associated with the metric. Use this to relate the metrics to an overall project, or the tables and columns or schema that are associated with the metric.','USER','dbo','TABLE','R_PPDM_METRIC_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PPDM REFERENCE VALUE TYPE: The specific value that is being measured, such as the region or location for the value. For example, the metric may measure the number of wells in the database, but the reference value may further refine this to indicate the number of wells in the Northern region.','USER','dbo','TABLE','R_PPDM_METRIC_REF_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE PPDM METRIC TYPE: the type of metric that is being measured, such the count of wells that have been quality controlled, the number of SW licenses that are in use, the time taken to complete an activity etc.','USER','dbo','TABLE','R_PPDM_METRIC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PPDM OBJECT STATUS: The current status of the object, such as enabled or disabled.','USER','dbo','TABLE','R_PPDM_OBJECT_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE PPDM OBJECT TYPE: The type of database object that is being tracked, such as a table, column, index, constraint or procedure.','USER','dbo','TABLE','R_PPDM_OBJECT_TYPE';
go

execute sp_addextendedproperty 'Description','PPDM OPERATING SYSTEM: Also known as an "OS," this is the software that communicates with computer hardware on the most basic level. Without an operating system, no software programs can run. The OS is what allocates memory, processes tasks, accesses disks and peripherials, and serves as the user interface. (Sharpened Glossary)','USER','dbo','TABLE','R_PPDM_OPERATING_SYSTEM';
go

execute sp_addextendedproperty 'Description','PPDM OWNER ROLE: a list of the roles that applications or business associates can play in the ownership of a group of tables and columns. For example, you may list the business value owner, the technical value owner, the data manager, the generating application, a using application and so on.','USER','dbo','TABLE','R_PPDM_OWNER_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE PPDM PROCEDURE TYPE: The type of procedure, such as stored, called, function etc.','USER','dbo','TABLE','R_PPDM_PROC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PPDM QUALITY CONTROL QUALITY: A value that indicates the quality of the data, whether the data is poor quality, fully verified, falls within expected range of values, requires further investigation etc.','USER','dbo','TABLE','R_PPDM_QC_QUALITY';
go

execute sp_addextendedproperty 'Description','REFERENCE PPDM QUALITY CONTROL STATUS: A valid type of quality control or validation status, such as batch loaded, visual inspection, verified by data analyst, verified by business analyst etc.','USER','dbo','TABLE','R_PPDM_QC_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE PPDM QUALITY CONTROL TYPE: The type of quality control that is being done, such as table level or column level quality control.','USER','dbo','TABLE','R_PPDM_QC_TYPE';
go

execute sp_addextendedproperty 'Description','PPDM RELATIONAL DATA BASE MANAGEMENT SYSTEM: RDBMS, short for relational database management system and pronounced as separate letters, a type of database management system (DBMS) that stores data in the form of related tables. Relational databases are powerful because they require few assumptions about how data is related or how it will be extracted from the database. As a result, the same database can be viewed in many different ways. An important feature of relational systems is that a single database can be spread across several tables. This differs fromflat-file databases, in which each database is self-contained in a single table. (ISP Glossary)','USER','dbo','TABLE','R_PPDM_RDBMS';
go

execute sp_addextendedproperty 'Description','REFERENCE REFERENCE VALUE TYPE: A list of the kinds of reference values that are used for declaration of business rules about data in your database. For example, you may state that the reference value type is the value of another column (such as in the case where the spud date must be after the well license date), or the case where if one column is populated, another must also be populated (if a production volume is entered, you must also enter a unit of measure).','USER','dbo','TABLE','R_PPDM_REF_VALUE_TYPE';
go

execute sp_addextendedproperty 'Description','PPDM ROW QUALILTY: A set of values indicating the quality of data in this row, usually with reference to the method or procedures used to load the data, although other types of quality reference are permitted.','USER','dbo','TABLE','R_PPDM_ROW_QUALITY';
go

execute sp_addextendedproperty 'Description','PPDM RULE CLASS: The class or type of rule, such as policy, practice, procedure, business rule.','USER','dbo','TABLE','R_PPDM_RULE_CLASS';
go

execute sp_addextendedproperty 'Description','REFERENCE PPDM RULE COMPONENT TYPE: The type of component that is associated with the rule.','USER','dbo','TABLE','R_PPDM_RULE_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','RULE DETAIL TYPE: the type of detail for a rule that is being captured, such as the minimum value that the column can store.','USER','dbo','TABLE','R_PPDM_RULE_DETAIL_TYPE';
go

execute sp_addextendedproperty 'Description','PPDM RULE PURPOSE: the objective or purpose for the business rule, such as data load quality control, management reporting etc.','USER','dbo','TABLE','R_PPDM_RULE_PURPOSE';
go

execute sp_addextendedproperty 'Description','PPDM RULE STATUS: The current status of a business rule, such as proposed, under review, approved, published, deprecated etc.','USER','dbo','TABLE','R_PPDM_RULE_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE RULE USE CONDITION TYPE: the type of condition that must be met for this procedure to be run. For example, the procedure may be used during inserts to the well table, during updates, during migrations, run monthly etc.','USER','dbo','TABLE','R_PPDM_RULE_USE_COND';
go

execute sp_addextendedproperty 'Description','PPDM RULE CROSS REFERENCE CONDITION: The type of condition that must be met before this cross reference comes into effect, usually used when you need to create a branched or dependent sequence of events. For example, one row of data will state that if the rule indicated as RULE_ID is successful, go to RULE_ID2. In another row of data, you can state that if the first rule has failed, a different RULE_ID2 would be in force.','USER','dbo','TABLE','R_PPDM_RULE_XREF_COND';
go

execute sp_addextendedproperty 'Description','PPDM RULE CROSS REFERENCE TYPE: The reason why a cross reference was created, such as an indication of a rule to be processed in the event that the first rule has been processed and passed or failed. In this case, the reference XREF TYPE may be RUN RULE ID2 if RULE ID passes (or fails). May also be used for rule management, such as rule that replaces an old rule, rule that must be implemented before or after another rule a rule that is a component of another rule, or to build the relationships between policies, practices, procedures and business rules.','USER','dbo','TABLE','R_PPDM_RULE_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','PPDM SCHEMA ENTITY TYPE: the type of schema element that is being described, such as element, attribute, element group etc.','USER','dbo','TABLE','R_PPDM_SCHEMA_ENTITY';
go

execute sp_addextendedproperty 'Description','REFERENCE SCHEMA GROUP TYPE: the type of grouping of schema entities that has been created, such as the relationship between an element and its attributes, parent child relationships, siblings, sequencing elements.','USER','dbo','TABLE','R_PPDM_SCHEMA_GROUP';
go

execute sp_addextendedproperty 'Description','REFERENCE PPDM SYSTEM TYPE: A list of valid types of systems, such as Relational Database, XML Schema, EDI, etc.','USER','dbo','TABLE','R_PPDM_SYSTEM_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE TABLE TYPE: the type of entry in this table that describes the physical implementation, such as table, view, materialized view etc.','USER','dbo','TABLE','R_PPDM_TABLE_TYPE';
go

execute sp_addextendedproperty 'Description','PPDM UNIT OF MEASURE ALIAS TYPE: The type of unit of measure alias or symbol that is referenced. In the sample data, the symbol types may be UTF8_SYMABOL, MIXED_CASE_SYMBOL, SINGLE_CASE_SYMBOL, PRINT_SYMBOL (used for representations that use special characters) or EPSG_SYMBOL (used for EPSG coordinate system references).','USER','dbo','TABLE','R_PPDM_UOM_ALIAS_TYPE';
go

execute sp_addextendedproperty 'Description','PPDM UNIT OF MEASURE USAGE: A list of the valid types of usage values, usually as defined by an authorized agency such as IEEE. Usual values would be NULL or CURRENT for current, deprecated, discouraged or strongly discouraged. Note that different reference sources may supply different values for usage. For example, API RP66 shows the BAR as a current unit of measure while the SI-10 shows it as deprecated.','USER','dbo','TABLE','R_PPDM_UOM_USAGE';
go

execute sp_addextendedproperty 'Description','PRESERVATION QUALITY TYPE: the quality of preservation for the samples used.','USER','dbo','TABLE','R_PRESERVE_QUALITY';
go

execute sp_addextendedproperty 'Description','PRESERVATION TYPE: the type of preservation for a lithologic fossil, such as abraded, crushed, broken, pyritized etc.','USER','dbo','TABLE','R_PRESERVE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PRODUCTION METHOD: This reference table identifies the method of production. For example swabbing, flowing, pumping or gas lift.','USER','dbo','TABLE','R_PRODUCTION_METHOD';
go

execute sp_addextendedproperty 'Description','PRODUCT COMPONENT TYPE: The type of component associated with a product','USER','dbo','TABLE','R_PRODUCT_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','PRODUCTION STRING COMPONENT TYPE: The type of component associated with a production string','USER','dbo','TABLE','R_PROD_STRING_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PRODUCTION STRING STATUS: A list of valid values for a production string status. This table allows you to capture status information from many points of view as it changes over time.','USER','dbo','TABLE','R_PROD_STRING_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE PRODUCTION STRING STATUS TYPE: The type of status reported for the production string. Can include construction status, operating status, producing status, abandonment status etc.','USER','dbo','TABLE','R_PROD_STRING_STAT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PRODUCTION STRING TYPE: Code indicating the type and/or status of the production string. The string could be abandoned, producing, disposal, injection, shut-in, etc..','USER','dbo','TABLE','R_PROD_STRING_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PRODUCTION STRING FORMATION STATUS: A list of valid status types for a production string formation, qualified by a status type column.','USER','dbo','TABLE','R_PROD_STR_FM_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE PRODUCTION STRING FORMATION STATUS TYPE: The type of status reported at the production string formation, such as completion status.','USER','dbo','TABLE','R_PROD_STR_FM_STAT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PROJECTION TYPE: A reference table identifying valid classifications of projections used by individual map projections. For example, Mercator, Lambert, Polyconic, ...','USER','dbo','TABLE','R_PROJECTION_TYPE';
go

execute sp_addextendedproperty 'Description','PROJECT ALIAS TYPE: The type of project alias that has been assigned. Could be the code assigned by an application or user, or by another organization.','USER','dbo','TABLE','R_PROJECT_ALIAS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PROJECT BUSINESS ASSOCIATE ROLE: the role of the business associate in the project, such as project manager, technical lead, DBA etc.','USER','dbo','TABLE','R_PROJECT_BA_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE PROJECT COPMPONENT REASON: the reason why business objects or other entities are related to this project. This may occur when one project is part of another project, when wells, land rights or seismc are related to a project etc.','USER','dbo','TABLE','R_PROJECT_COMP_REASON';
go

execute sp_addextendedproperty 'Description','PROJECT COMPONENT TYPE: the reason the component is associated with the project, such as created for, contract that governs, used during etc.','USER','dbo','TABLE','R_PROJECT_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PROJECT CONDITION: This table lists the type of condition(s) that must exist before the project or step can be started. May be an external condition (a facility to be shut down) or another step in the project that must be completed.','USER','dbo','TABLE','R_PROJECT_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE PROJECT EQUIPMENT ROLE: the role played by a piece of equipment in a project, such as pumper, saftey equipment, processor, primary storage device etc.','USER','dbo','TABLE','R_PROJECT_EQUIP_ROLE';
go

execute sp_addextendedproperty 'Description','PROJECT or PROJECT STEP STATUS: the status of a project or a step in a project. May include underway, on hold, completed, cancelled.','USER','dbo','TABLE','R_PROJECT_STATUS';
go

execute sp_addextendedproperty 'Description','PROJECT or PROJECT STEP STATUS TYPE: The type of status, or perspective from which the status is observed, such as financial, operational, legal etc.','USER','dbo','TABLE','R_PROJECT_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PROJECT TYPE: the type of project, such as seismic, geological, exploitation etc.','USER','dbo','TABLE','R_PROJECT_TYPE';
go

execute sp_addextendedproperty 'Description','PROJECT STEP TYPE: The type of step for the project.','USER','dbo','TABLE','R_PROJ_STEP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PROJECT STEP CROSS REFERENCE REASON: describes relationships between steps in a project. May define necessary precursors, following steps, alternate paths etc.','USER','dbo','TABLE','R_PROJ_STEP_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PROPPANT AGENT TYPE: This reference table identifies the type of proppant used in the hydraulic fracture treatment fluid. For example, fracturing sands, resin-coated ceramic proppants, sintered bauxites...','USER','dbo','TABLE','R_PROPPANT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE PUBLICATION NAME: the name of the publication that was ref erenced, such as the Oil and Gas Journal.','USER','dbo','TABLE','R_PUBLICATION_NAME';
go

execute sp_addextendedproperty 'Description','QUALIFIER TYPE: the type of qualifier, relative to the qualifier described. Could be a diversity qualifier, species qualifier etc.','USER','dbo','TABLE','R_QUALIFIER_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE QUALITY: A reference table describing standard quality indicators. For example poor, good, very good or excellent.','USER','dbo','TABLE','R_QUALITY';
go

execute sp_addextendedproperty 'Description','REFERENCE RATE CONDITION: A list of conditions that are applied to a rate schedule. For example a road access rate may only apply when a well is in production or during drilling operations.','USER','dbo','TABLE','R_RATE_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE RATE SCHEDULE TYPE: the type of schedule, such as a regulatory bodys lessor schedule for rental payments.','USER','dbo','TABLE','R_RATE_SCHEDULE';
go

execute sp_addextendedproperty 'Description','RATE SCHEDULE COMPONENT TYPE: The type of component associated with a rate schedule','USER','dbo','TABLE','R_RATE_SCHEDULE_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','RATE SCHEDULE CROSS REFERENCE TYPE: the reason why the rate schedules have been cross referenced. A new schedule may replace an older one, or a supplementary schedule may be defined etc.','USER','dbo','TABLE','R_RATE_SCHED_XREF';
go

execute sp_addextendedproperty 'Description','R RATE TYPE: the type of rate charged in this schedule. Examples may include posting fees, expenses, rentals etc.','USER','dbo','TABLE','R_RATE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE RATIO CURVE TYPE: A reference table identifying the type of ratio curve that is used in decline curve forecast calculations such as linear, semi-log, log-log, etc.','USER','dbo','TABLE','R_RATIO_CURVE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE RATIO FLUID TYPE: A reference table identifying the type of ratio fluid that is used in decline curve forecast calculations for GOR, YIELD, etc.','USER','dbo','TABLE','R_RATIO_FLUID_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE TEST RECORDER POSITION: This reference table describes position of a test recorder or gauge relative to the test tool components. For example below or above straddle.','USER','dbo','TABLE','R_RECORDER_POSITION';
go

execute sp_addextendedproperty 'Description','REFERENCE TEST RECORDER TYPE: This reference table describes various types or recorders or pressure gauges. For example bourbon tube, quartz gauge or strain gauge.','USER','dbo','TABLE','R_RECORDER_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE REMARK TYPE: A reference table describing remark types. This is an open reference table commonly used to group remarks. For example drilling, geologists, regulatory or planning.','USER','dbo','TABLE','R_REMARK_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE REMARK USE TYPE: Indicates the type of use that a remark should be put to, such as for internal use only, for external publication etc.','USER','dbo','TABLE','R_REMARK_USE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE REPEAT STRATIGRAPHY TYPE: The reason the stratigraphic unit was picked twice in the same interpretation. Usually occurs because of horizontal or deviated drilling techniques or as a result of faulting or folding.','USER','dbo','TABLE','R_REPEAT_STRAT_TYPE';
go

execute sp_addextendedproperty 'Description','HIERARCHY COMPONENT TYPE: The type of business object that has been associated with a level in the hierarchy. Could be field, lease, area of interest, geographic area etc.','USER','dbo','TABLE','R_REPORT_HIER_COMP';
go

execute sp_addextendedproperty 'Description','HIERARCHY LEVEL TYPE: The type of level that has been defined in the hierarchy, such as lease, pool etc.','USER','dbo','TABLE','R_REPORT_HIER_LEVEL';
go

execute sp_addextendedproperty 'Description','HIERARCHY TYPE: The type of hierarchy that has been created.','USER','dbo','TABLE','R_REPORT_HIER_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE REPORTING HIERARCHY ALIAS TYPE: This reference table describes the type of alias. For example a well may be assigned a government alias, contract alias or project alias.','USER','dbo','TABLE','R_REP_HIER_ALIAS_TYPE';
go

execute sp_addextendedproperty 'Description','RESERVE ENTITY COMPONENT TYPE: The type of component associated with a reserve entity','USER','dbo','TABLE','R_RESENT_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','RESERVE ENTITY CONFIDENCE TYPE: The level of confidence associated with this reserve class, such as proven, possible, probable.','USER','dbo','TABLE','R_RESENT_CONFIDENCE';
go

execute sp_addextendedproperty 'Description','RESERVE ENTITY GROUPING CRITERIA TYPE: the criteria used to group reserve entities, such as aeria extent, lease based etc.','USER','dbo','TABLE','R_RESENT_GROUP_TYPE';
go

execute sp_addextendedproperty 'Description','REVISION CATEGORY TYPE: The types of revision categories which have been defined. Used for grouping revision reasons into more generalized categories such as ADDITIONS and REVISIONS','USER','dbo','TABLE','R_RESENT_REV_CAT';
go

execute sp_addextendedproperty 'Description','RESERVE ENTITY USE TYPE: Differentiates between reserve classes where the client expects to enter data, i.e. Proved Developed Producing, from those reserve classes which are defined for reporting purposes only, i.e. Proved plus 1/2 Probable.','USER','dbo','TABLE','R_RESENT_USE_TYPE';
go

execute sp_addextendedproperty 'Description','RESERVE ENTITY VOLUME METHOD: The method used by the analyst to establish reserve volumes, such as decline analysis, material balance or volumetric calculations.','USER','dbo','TABLE','R_RESENT_VOL_METHOD';
go

execute sp_addextendedproperty 'Description','RESERVE ENTITY CROSS REFERENCE TYPE: The type of relationships that exists between two reserve entities. Examples are contains, replaces, adjacent etc.','USER','dbo','TABLE','R_RESENT_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE RESTRICTED ACTIVITY: The activity that is restricted, such as on land conventional operations, drilling etc.','USER','dbo','TABLE','R_REST_ACTIVITY';
go

execute sp_addextendedproperty 'Description','REFERENCE RESTRICTION DURATION TYPE: whether the restriction is active for durations that are seasonal, permanent, variable, event driven, date driven etc.','USER','dbo','TABLE','R_REST_DURATION';
go

execute sp_addextendedproperty 'Description','LAND RESTRICTION REMARK: Remrks about a land restriction that have been coded can be entered using this reference table. This allows regulatory agencies to list which remarks are relevant to the restriction.','USER','dbo','TABLE','R_REST_REMARK';
go

execute sp_addextendedproperty 'Description','REFERENCE LAND RESTRICTION TYPE: whether the restriction is on the surface, for minerals etc.','USER','dbo','TABLE','R_REST_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE RETENTION PERIOD: The length of time records or materials should be kept in a certain location or form for administrative, legal, fiscal, historical, or other purposes. Retention periods are determined by balancing the potential value of the information to the agency against the costs of storing the records containing that information. Retention periods are often set for record series, but specific records within that series may need to be retained longer because they are required for litigation or because circumstances give those records unexpected archival value.','USER','dbo','TABLE','R_RETENTION_PERIOD';
go

execute sp_addextendedproperty 'Description','REVISOIN METHOD TYPE: The method used to calculate the revised volumes, such as decline analysis, materials balance, volumetric analysis etc.','USER','dbo','TABLE','R_REVISION_METHOD';
go

execute sp_addextendedproperty 'Description','BLOWOUT PREVENTOR TYPE: A large valve at the top of a well that may be closed if the drilling crew loses control of formation fluids. By closing this valve (usually operated remotely via hydraulic actuators), the drilling crew usually regains control ofthe reservoir, and procedures can then be initiated to increase the mud density until it is possible to open the BOP and retain pressure control of the formation. BOPs come in a variety of styles, sizes and pressure ratings. Some can effectively close over an open wellbore, some are designed to seal around tubular components in the well (drillpipe, casing or tubing) and others are fitted with hardened steel shearing surfaces that can actually cut through drillpipe. Since BOPs are critically important to the safety of the crew, the rig and the wellbore itself, BOPs are inspected, tested and refurbished at regular intervals determined by a combination of risk assessment, local practice, well type and legal requirements. BOP tests vary from daily function testing on critical wells to monthly or less frequent testing on wells thought to have low probability of well control problems. (Schlumberger Oilfield Glossary). Usual values include single, double, shear, ram.','USER','dbo','TABLE','R_RIG_BLOWOUT_PREVENTER';
go

execute sp_addextendedproperty 'Description','REFERENCE RIG CATEGORY: The category of the rig, describing its basic purpose. Typical examples include drilling rig, completion rig, service rig, wireline rig, workover rig, rathole rig etc.','USER','dbo','TABLE','R_RIG_CATEGORY';
go

execute sp_addextendedproperty 'Description','REFERENCE RIG CLASS: Indicates the class of rig, such as single, super single, double, triple.','USER','dbo','TABLE','R_RIG_CLASS';
go

execute sp_addextendedproperty 'Description','REFERENCE RIG CODE: This reference table defines the unique codes assigned to each rig or installation. For example, the code "GA" may be assigned to the drilling rig Glomar Arctic III.','USER','dbo','TABLE','R_RIG_CODE';
go

execute sp_addextendedproperty 'Description','DESANDER TYPE:A hydrocyclone device that removes large drill solids from the whole mud system. The desander should be located downstream of the shale shakers and degassers, but before the desilters or mud cleaners. A volume of mud is pumped into the wide upper section of the hydrocylone at an angle roughly tangent to its circumference. As the mud flows around and gradually down the inside of the cone shape, solids are separated from the liquid by centrifugal forces. The solids continue around and down until they exit the bottom of the hydrocyclone (along with small amounts of liquid) and are discarded. The cleaner and lighter density liquid mud travels up through a vortex in the center of the hydrocyclone, exits through piping at the top of the hydrocyclone and is then routed to the mud tanks and the next mud-cleaning device, usually a desilter. Various size desander and desilter cones are functionally identical, with the size of the cone determining the size of particles the device removes from the mud system. (Schlumberger Oilfield Glossary).','USER','dbo','TABLE','R_RIG_DESANDER_TYPE';
go

execute sp_addextendedproperty 'Description','DESILTER TYPE: A hydrocyclone much like a desander except that its design incorporates a greater number of smaller cones. As with the desander, its purpose is to remove unwanted solids from the mud system. The smaller cones allow the desilter to efficiently remove smaller diameter drill solids than a desander does. For that reason, the desilter is located downstream from the desander in the surface mud system. (Schlumberger Oilfield Glossary).','USER','dbo','TABLE','R_RIG_DESILTER_TYPE';
go

execute sp_addextendedproperty 'Description','DRAWWORKS TYPE: The machine on the rig consisting of a large-diameter steel spool, brakes, a power source and assorted auxiliary devices. The primary function of the drawworks is to reel out and reel in the drilling line, a large diameter wire rope, in a controlled fashion. The drilling line is reeled over the crown block and traveling block to gain mechanical advantage in a "block and tackle" or "pulley" fashion. This reeling out and in of the drilling line causes the traveling block, and whatever may be hanging underneath it, to be lowered into or raised out of the wellbore. The reeling out of the drilling line is powered by gravity and reeling in by an electric motor or diesel engine.(Schlumberger Oilfield Glossary).','USER','dbo','TABLE','R_RIG_DRAWWORKS';
go

execute sp_addextendedproperty 'Description','GENERATOR or PLANT TYPE: The type of generator or plant, such as lighting or compressor.','USER','dbo','TABLE','R_RIG_GENERATOR_TYPE';
go

execute sp_addextendedproperty 'Description','HOOK or HOOKBLOCK TYPE: The high-capacity J-shaped equipment used to hang various other equipment, particularly the swivel and kelly, the elevator bails or topdrive units. The hook is attached to the bottom of the traveling block and provides a way to pickup heavy loads with the traveling block. The hook is either locked (the normal condition) or free to rotate, so that it may be mated or decoupled with items positioned around the rig floor, not limited to a single direction. (Schlumberger Oilfield Glossary)','USER','dbo','TABLE','R_RIG_HOOKBLOCK_TYPE';
go

execute sp_addextendedproperty 'Description','MAST TYPE: The structure used to support the crown blocks and the drillstring. Masts are usually rectangular or trapezoidal in shape and offer a very good stiffness, important to land rigs whose mast is laid down when the rig is moved. They suffer from being heavierthan conventional derricks and consequently are not usually found in offshore environments, where weight is more of a concern than in land operations. (Schlumberger Oilfield Glossary).','USER','dbo','TABLE','R_RIG_MAST';
go

execute sp_addextendedproperty 'Description','RIG OVERHEAD EQUIPMENT CAPACITY TYPE: The type of capacity for the overhead equipmetn, such as static or dynamic.','USER','dbo','TABLE','R_RIG_OVERHEAD_CAPACITY';
go

execute sp_addextendedproperty 'Description','RIG OVERHEAD EQUIPMENT TYPE: The class or type over overhead equipment, such as travelling block or swivel.','USER','dbo','TABLE','R_RIG_OVERHEAD_TYPE';
go

execute sp_addextendedproperty 'Description','PUMP TYPE: The pump on a rig is used to circulate materials in the well bore. Use this table to describe the type of pump.','USER','dbo','TABLE','R_RIG_PUMP';
go

execute sp_addextendedproperty 'Description','PUMP FUNCTION: The function filled by the pump.','USER','dbo','TABLE','R_RIG_PUMP_FUNCTION';
go

execute sp_addextendedproperty 'Description','SUBSTRUCTURE: Foundation on which the derrick and engines sit. Contains space for storage and well control equipment. (http://www.oilonline.com/info/glossary.asp)','USER','dbo','TABLE','R_RIG_SUBSTRUCTURE';
go

execute sp_addextendedproperty 'Description','SWIVEL TYPE: A mechanical device that must simultaneously suspend the weight of the drillstring, provide for rotation of the drillstring beneath it while keeping the upper portion stationary, and permit high-volume flow of high-pressure drilling mud from the fixed portion to the rotating portion without leaking. (Schlumberger Oilfield Glossary)','USER','dbo','TABLE','R_RIG_SWIVEL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE RIG TYPE: A reference table describing the type of rig installation. For example land, barge, submersible, platform, jackup, drillship, semisubmersible or artificial gravel island...','USER','dbo','TABLE','R_RIG_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE RECORDS MANAGEMENT INFORMATION ITEM CONTACT TYPE: The type of contact for an information item, other than authorship. For example, you may need to capture the contact for purchase, the contact for authorization to access the information or the contact for obtaining updates.','USER','dbo','TABLE','R_RMII_CONTACT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE RECORDS MANAGEMENT INFORMATION ITEM TYPE: The type of content associated with a records management information item','USER','dbo','TABLE','R_RMII_CONTENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE RECORDS MANAGEMENT INFORMATION ITEM DEFICIENCY TYPE: a list of the types of deficiencies that may be noted on an information item.','USER','dbo','TABLE','R_RMII_DEFICIENCY';
go

execute sp_addextendedproperty 'Description','REFERENCE RECORDS MANAGEMENT INFORMATION ITEM DESCRIPTION TYPE: The type of description for the information item. Could be a scale reference, type of report, classification, size or anything needed.','USER','dbo','TABLE','R_RMII_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE RECORDS MANAGEMENT INFORMATION ITEM GROUP TYPE: The type of information item group, such as a well file, that consists of a set of documents or other products which together serve a useful business function. If you use the Dublin Core terms, valueswould be conformsTo, hasFormat, hasPart, hasVersion, isFormatOf, isPartOf, isReferencedBy, isReplacedBy, isRequiredBy, isVersionOf, references, replaces, requires.','USER','dbo','TABLE','R_RMII_GROUP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE RECORDS MANAGEMENT INFORMATION ITEM META DATA CODE: A code assigned to a meta data value, often set by the creators of a meta data standard.','USER','dbo','TABLE','R_RMII_METADATA_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE RECORDS MANAGEMENT INFORMATION ITEM METADATA CODE TYPE: The category or type of information, as defined in meta data (such as Dublin Core, FGDC or ISO) that is being stored.','USER','dbo','TABLE','R_RMII_METADATA_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE RECORDS MANAGEMENT INFORMATION ITEM QUALITY CODE: A code indicating the quality of the information contained on the information item.','USER','dbo','TABLE','R_RMII_QUALITY_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE RECORDS MANAGEMENT INFORMATION ITEM STATUS: within the domain of a specific STATUS TYPE, the status of an information item. The status of information may differ from the status of its physical representation in some cases, such as information that is subject to updates or renewals.','USER','dbo','TABLE','R_RMII_STATUS';
go

execute sp_addextendedproperty 'Description','RECORDS MANAGEMENT INFORMATION ITEM STATUS TYPE: The type of status that is being defined, such as the purchase order status for a subscription, the renewal status, or the approval status of the information.','USER','dbo','TABLE','R_RMII_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE THESAURUS CROSS REFERENCE TYPE: Use this value to indicate the relationship between words in two thesaurii, such as replaces previous value, exact semantic meaning, sub type of semantic meaning, super type of semantic meaning, partial overlap in meaning, completely different meanings (use when the words are the same, but the semantic does not correrspond).','USER','dbo','TABLE','R_RM_THESAURUS_XREF';
go

execute sp_addextendedproperty 'Description','REFERENCE ROAD CONDITION: a valid list of road conditions. Road conditions provide important environmental, safety and billing information. A sample list of values may be found at http://www.highways.gov.sk.ca/docs/travelers_info/road_terminology.asp','USER','dbo','TABLE','R_ROAD_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE ROAD CONTROL TYPE: The type of controls in place to access a road, such as radio controls for well site access roads. Often, this is done for safety reasons.','USER','dbo','TABLE','R_ROAD_CONTROL_TYPE';
go

execute sp_addextendedproperty 'Description','ROAD DRIVING SIDE: Indicates the side of the road that you drive on, either left (Canada, US, Europe) or right (UK, Australia, Japan).','USER','dbo','TABLE','R_ROAD_DRIVING_SIDE';
go

execute sp_addextendedproperty 'Description','REFERENCE ROAD TRAFFIC FLOW TYPE: The type of traffic flow for a road, usually one way, two way or reversible (traffic flow changes during the day or based on some other business rule).','USER','dbo','TABLE','R_ROAD_TRAFFIC_FLOW_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE ROCK CLASSIFICATION SCHEME: The rock classification scheme used to name the rock type. For example, the Folk classification scheme uses the terms, arkose, quartzarenite, etc. The Dunham classification scheme uses the terms mudstone, wackestone, etc.','USER','dbo','TABLE','R_ROCK_CLASS_SCHEME';
go

execute sp_addextendedproperty 'Description','ROLL ALONG METHOD: for seismic data, the type of roll along used for field recording. May be 4 station switch, fixed for patch, fixed for seis set etc.','USER','dbo','TABLE','R_ROLL_ALONG_METHOD';
go

execute sp_addextendedproperty 'Description','R ROYALTY TYPE: a type of royalty, such as gross overriding, net overriding, net profit interest, net carried interest. A royalty is a payment made to a party or a jurisdiction according to the terms of an agreement.','USER','dbo','TABLE','R_ROYALTY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SALINITY TYPE: This reference tables identifies the type of water salinity measurement techniques used for well tests and sample analysis. For example chlorides or total dissolved solids.','USER','dbo','TABLE','R_SALINITY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SAMPLE ANALYSIS COLLECTION TYPE: The type of collection reason used for gathering this sample, such as for daily tests, weekly tests, spot analysis etc.','USER','dbo','TABLE','R_SAMPLE_COLLECTION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SAMPLE COLLECTION METHOD: a list of the valid methods used to collect samples, such as coring or sidewall coring, dipping, cutting collection etc.','USER','dbo','TABLE','R_SAMPLE_COLLECT_METHOD';
go

execute sp_addextendedproperty 'Description','SAMPLE COMPONENT TYPE: The type of component associated with a sample.','USER','dbo','TABLE','R_SAMPLE_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','SAMPLE CONTAMINANT: A contaminant in a sample that may affect later analysis. Drilling mud may be considered to be a contaminant.','USER','dbo','TABLE','R_SAMPLE_CONTAMINANT';
go

execute sp_addextendedproperty 'Description','REFERENCE DESCRIPTION CODE: A codified value assigned to the description of a sample, usually but not always, according to a coded reference list provided in scientific literature.','USER','dbo','TABLE','R_SAMPLE_DESC_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE DESCRIPTION TYPE: the type of description for the sample that is not included in the SAMPLE table, the SAMPLE LITH DESC table or another table in the model. This table will support additional, less common description types.','USER','dbo','TABLE','R_SAMPLE_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SAMPLE FRACTION TYPE: The type of fraction method used to separate the sample into two or more samples during this step. This process could involve phase or chemical separation. A homogeneous separation is possible in cases where the sample is to be subject to different kinds of analysis.','USER','dbo','TABLE','R_SAMPLE_FRACTION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SAMPLE LOCATION: A reference table describing the various locations that samples can be extracted from. For example shaker, mud pit, core or wellbore.','USER','dbo','TABLE','R_SAMPLE_LOCATION';
go

execute sp_addextendedproperty 'Description','REFERENCE SAMPLE PHASE TYPE: The phase of a sample, usually solid, gas or liquid. The phase of a sample may change from its original location as it is collected, stored and analyzed (e.g. methane, ethane, propane). Note: During well depletion the pressure will drop which can result in a phase change, this is a phase change at collection. C1 to C3 hydrocarbons may phase change during storage if the temperature and pressure are different from that of the well, this is phase change at storage. Pyrolysis results in a phase change of the sample during analysis.','USER','dbo','TABLE','R_SAMPLE_PHASE';
go

execute sp_addextendedproperty 'Description','SAMPLE PREPARATION CLASS: the type or class of preparation for the sample, such as a chemical wash, thin section, acid wash etc. Specific methods are stored in ANALYSIS_STEP.','USER','dbo','TABLE','R_SAMPLE_PREP_CLASS';
go

execute sp_addextendedproperty 'Description','REFERENCE SAMPLE REFERENCE VALUE TYPE: the kind of value that is used to compare a description to. For example, could be a color when originally collected (reference value type is ORIGINAL COLLECTION TIME) , the degree of soil compaction at various collection depths (reference value type is SAMPLE COLLECTION DEPTH) etc.','USER','dbo','TABLE','R_SAMPLE_REF_VALUE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SAMPLE SHAPE: The shape of the sample, such as cylindrical, square, oblong, amorphous, rectangular, slice, random etc.','USER','dbo','TABLE','R_SAMPLE_SHAPE';
go

execute sp_addextendedproperty 'Description','SAMPLE TYPE: The type of sample that is described. For example, a cutting.','USER','dbo','TABLE','R_SAMPLE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SCALE TRANSFORM TYPE: This reference table identifies the type of scaling transform. For example, linear, log, compressed or hybrid.','USER','dbo','TABLE','R_SCALE_TRANSFORM';
go

execute sp_addextendedproperty 'Description','REFERENCE SCREEN LOCATION: Shakers typically contain three or more screens, each of which progressively has a smaller mesh size. This table indicates the relative position of each screen in the shaker, usually top, middle or bottom.','USER','dbo','TABLE','R_SCREEN_LOCATION';
go

execute sp_addextendedproperty 'Description','REFERENCE SECTION TYPE: A reference table identifying valid types of section or equivalent blocks. For example, block, bay, survey, militia donation, Michigan road land section, ...','USER','dbo','TABLE','R_SECTION_TYPE';
go

execute sp_addextendedproperty 'Description','SEISMIC PATH: Whether the path measured is one way or two way.','USER','dbo','TABLE','R_SEISMIC_PATH';
go

execute sp_addextendedproperty 'Description','REFERENCE SEISMIC THREE DIMENSION TYPE: The type of 3D that has been completed, such as broadside, side shoot, inline etc.','USER','dbo','TABLE','R_SEIS_3D_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SEISMIC ACTIVITY CLASS: A class or group of activity types related to seismic data. Examples include planning activities, acquisition activities, processing activities, records management activities etc.','USER','dbo','TABLE','R_SEIS_ACTIVITY_CLASS';
go

execute sp_addextendedproperty 'Description','REFERENCE SEISMIC ACTIVITY: A Kind of activity related to seismic data. Activites are qualified by the class of activity (R SEIS ACTIVITY CLASS) they are engaged in, such as planning activities, acquisition activities, processing activities, records management activities etc.','USER','dbo','TABLE','R_SEIS_ACTIVITY_TYPE';
go

execute sp_addextendedproperty 'Description','SEISMIC AUTHORIZE LIMITATION: Limitation that is associated with the seismic authorization. Typical examples would include - requires chief geophysicists signature, no limitation.','USER','dbo','TABLE','R_SEIS_AUTHORIZE_LIMIT';
go

execute sp_addextendedproperty 'Description','SEISMIC AUTHORIZATION REASON: the reason why this authorization was granted, such as legislated, area not of interest, old data etc.','USER','dbo','TABLE','R_SEIS_AUTHORIZE_REASON';
go

execute sp_addextendedproperty 'Description','SEISMIC AUTHORIZATION TYPE: The type of authorization that is granted. Examples are: all data, raw data only, paper copies of all data, to pre-processing only etc.','USER','dbo','TABLE','R_SEIS_AUTHORIZE_TYPE';
go

execute sp_addextendedproperty 'Description','BIN METHOD: the method used to create seismic bins, such as rectilinear, flex binning with radius of 4 bins, bin borrowing etc.','USER','dbo','TABLE','R_SEIS_BIN_METHOD';
go

execute sp_addextendedproperty 'Description','BIN OUTLINE TYPE: The type of outline described, such as the outline to the extent of partial coverage, or the outline to the extent of full coverage only.','USER','dbo','TABLE','R_SEIS_BIN_OUTLINE_TYPE';
go

execute sp_addextendedproperty 'Description','CABLE MAKE: The name and model of the cable that was used in the streamer.','USER','dbo','TABLE','R_SEIS_CABLE_MAKE';
go

execute sp_addextendedproperty 'Description','CHANNEL TYPE: The type of channel that is used for recording the seismic data. May be time break channel, uphole channel, data channel etc.','USER','dbo','TABLE','R_SEIS_CHANNEL_TYPE';
go

execute sp_addextendedproperty 'Description','DIMENSION: The dimension or geometry of the seismic data. Mmay be 1D, 2D, 3D, swath, 3D water bottom','USER','dbo','TABLE','R_SEIS_DIMENSION';
go

execute sp_addextendedproperty 'Description','R SEISMIC ENERGY TYPE: Describes the type of seismic energy source used','USER','dbo','TABLE','R_SEIS_ENERGY_TYPE';
go

execute sp_addextendedproperty 'Description','SEISMIC POINT FLOW DESCRIPTION TYPE: The type of remark made about a flowing seismic hole. Typical examples include description of the flow, description of remedial actions, description of the damage.','USER','dbo','TABLE','R_SEIS_FLOW_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','SEISMIC GROUP TYPE: The type of group that is created. Groups can be created to associate a survey with the 2D or 3D sets acquired with it, to indicate what sets are combined into a processing or interepretation data set or to associate lines and segment','USER','dbo','TABLE','R_SEIS_GROUP_TYPE';
go

execute sp_addextendedproperty 'Description','SEISMIC INSPECTION COMPONENT TYPE: The type of component assocaited with a seismic inspection','USER','dbo','TABLE','R_SEIS_INSP_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','SEISMIC LICENSE CONDITION TYPE: The type of condition that is applied to a seismic or geophysical license. Could include stipulations about disposal, timber salvage, inspections, compliance with legislation etc.','USER','dbo','TABLE','R_SEIS_LIC_COND';
go

execute sp_addextendedproperty 'Description','SEISMIC LICENSE CONDITION CODE: A validated set of codes for a condition type.','USER','dbo','TABLE','R_SEIS_LIC_COND_CODE';
go

execute sp_addextendedproperty 'Description','DUE CONDITION: The state that must be achieved for the condition to become effective. For example, a report may be due 60 days after operations commence (or cease).','USER','dbo','TABLE','R_SEIS_LIC_DUE_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE LICENSE VIOLATION RESOLUTION TYPE: The type of resolution to a violation of a license term, such as the payment of a fine or creation of new processes.','USER','dbo','TABLE','R_SEIS_LIC_VIOL_RESOL';
go

execute sp_addextendedproperty 'Description','REFERENCE VIOLATION TYPE: The type of violation of a license that is being recorded. Can be as simple as failure to submit necessary reports or something more difficult such as improper procedures.','USER','dbo','TABLE','R_SEIS_LIC_VIOL_TYPE';
go

execute sp_addextendedproperty 'Description','SEISMIC PARAMETER ORIGIN:  The origin or source of the parameters used in the software application.','USER','dbo','TABLE','R_SEIS_PARM_ORIGIN';
go

execute sp_addextendedproperty 'Description','PATCH TYPE: The type of seismic patch that was used in recording, such as split spread, end-on, grid etc.','USER','dbo','TABLE','R_SEIS_PATCH_TYPE';
go

execute sp_addextendedproperty 'Description','SEISMIC PICK METHOD: The method that was used to make the seismic picks, such as automatic picking, manual picking, semi-automated picking etc.','USER','dbo','TABLE','R_SEIS_PICK_METHOD';
go

execute sp_addextendedproperty 'Description','PROCESSING COMPONENT TYPE: The type of processing component, such as seismic line, velocity survey etc.','USER','dbo','TABLE','R_SEIS_PROC_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','SEISMIC PROCESSING PARAMETER TYPE NAME: the type or name of the processing parameter applied at this step, such as filter, gain etc.','USER','dbo','TABLE','R_SEIS_PROC_PARM';
go

execute sp_addextendedproperty 'Description','PROCESSING SET TYPE: the type of processing set, such as 3D set, recon processing etc.','USER','dbo','TABLE','R_SEIS_PROC_SET_TYPE';
go

execute sp_addextendedproperty 'Description','PROCESSING STATUS: the status of processing, either for the step or the complete processing step. Could be waiting for data, complete, cancelled etc.','USER','dbo','TABLE','R_SEIS_PROC_STATUS';
go

execute sp_addextendedproperty 'Description','SEISMIC PROCESS STEP NAME: A list of valid values for seismic processing step names.','USER','dbo','TABLE','R_SEIS_PROC_STEP_NAME';
go

execute sp_addextendedproperty 'Description','SEISMIC PROCESS STEP TYPE: the type of processing step completed, such as migration, stack, flattening etc.','USER','dbo','TABLE','R_SEIS_PROC_STEP_TYPE';
go

execute sp_addextendedproperty 'Description','RECORDING FORMAT TYPE: May be analog, SEG B, SEG Y etc','USER','dbo','TABLE','R_SEIS_RCRD_FMT_TYPE';
go

execute sp_addextendedproperty 'Description','SEISMIC RECORDING INSTRUMENTS MAKE: make and model of the equipment used.','USER','dbo','TABLE','R_SEIS_RCRD_MAKE';
go

execute sp_addextendedproperty 'Description','ARRAY TYPE: the type of receiver array used','USER','dbo','TABLE','R_SEIS_RCVR_ARRY_TYPE';
go

execute sp_addextendedproperty 'Description','RECEIVER TYPE: May be geophone, hydrophone','USER','dbo','TABLE','R_SEIS_RCVR_TYPE';
go

execute sp_addextendedproperty 'Description','RECORD TYPE: the type of seismic record, such as good record, bad data, test record etc.','USER','dbo','TABLE','R_SEIS_RECORD_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE DATUM: The datum to which depths have been corrected. For marine recording, this may be Mean Sea Level (MSL).','USER','dbo','TABLE','R_SEIS_REF_DATUM';
go

execute sp_addextendedproperty 'Description','REPORTED REFERENCE NUMBER TYPE: The type of reference number associated with the seismic item that is being catalogued, such as a shot point number, trace number or file number.','USER','dbo','TABLE','R_SEIS_REF_NUM_TYPE';
go

execute sp_addextendedproperty 'Description','SAMPLE TYPE: The type of sample that is captured in this product. Usually either time or depth.','USER','dbo','TABLE','R_SEIS_SAMPLE_TYPE';
go

execute sp_addextendedproperty 'Description','REASON: The reason the segment was created. May be acquisition, processing, interpretation, data partnership, data sale or transaction','USER','dbo','TABLE','R_SEIS_SEGMENT_REASON';
go

execute sp_addextendedproperty 'Description','SEISMIC SET COMPONENT TYPE: The type of component associated with a seismic set','USER','dbo','TABLE','R_SEIS_SET_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','SPECTRUM: Indicates additional types of measurements taken. May be magnetic, electromagnetic, gravity, shearwave...','USER','dbo','TABLE','R_SEIS_SPECTRUM_TYPE';
go

execute sp_addextendedproperty 'Description','SOURCE ARRAY TYPE: linear, circular, star','USER','dbo','TABLE','R_SEIS_SRC_ARRAY_TYPE';
go

execute sp_addextendedproperty 'Description','SEISMIC SOURCE INSTRUMENTS MAKE: make and model of the equipment used.','USER','dbo','TABLE','R_SEIS_SRC_MAKE';
go

execute sp_addextendedproperty 'Description','SEISMIC STATION TYPE: seismic station type such as CDP, source, receiver etc','USER','dbo','TABLE','R_SEIS_STATION_TYPE';
go

execute sp_addextendedproperty 'Description','SEISMIC STATUS: the status of the seismic set, such as underway, complete. May also represent an ownership status (trade, proprietary, spec etc.)','USER','dbo','TABLE','R_SEIS_STATUS';
go

execute sp_addextendedproperty 'Description','STATUS TYPE: The type of status of the seismic set. Typical status might include ownership (trade, proprietary, spec shoot) or acquisition status (planning, approved, active, complete).','USER','dbo','TABLE','R_SEIS_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SEISMIC SUMMARY TYPE: The reason why or kind of seismic summary that has been created, such as a mapping summary, one based on CDP, one based on ownership or some kind of activity.','USER','dbo','TABLE','R_SEIS_SUMMARY_TYPE';
go

execute sp_addextendedproperty 'Description','SWEEP TYPE: The kind of vibroseis sweek. May be linear, variable','USER','dbo','TABLE','R_SEIS_SWEEP_TYPE';
go

execute sp_addextendedproperty 'Description','SEISMIC TRANSACTION COMPONENT TYPE: The type of component associated with a seismic transaction','USER','dbo','TABLE','R_SEIS_TRANS_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SEND METHOD: The method used to send an object from a sender to a receiver. Could be by registered mail, courier, hand delivery, email etc.','USER','dbo','TABLE','R_SEND_METHOD';
go

execute sp_addextendedproperty 'Description','R SERVICE QUALITY: The quality of service provided by this BUSINESS ASSOCIATE, either for a specified address, a service or a service at an address.','USER','dbo','TABLE','R_SERVICE_QUALITY';
go

execute sp_addextendedproperty 'Description','REFERENCE SEVERITY: A vaild list of severity types used for a qualifier of lost circulation or water flow into the wellbore. For example minor or severe.','USER','dbo','TABLE','R_SEVERITY';
go

execute sp_addextendedproperty 'Description','REFERENCE SUPPORT FACILITY AIRSTRIP TYPE: A list of valid types of airstrips.','USER','dbo','TABLE','R_SF_AIRSTRIP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SUPPORT FACILITY BRIDGE TYPE: A valid type of bride, such as permanent steel, wood, winter ice etc.','USER','dbo','TABLE','R_SF_BRIDGE_TYPE';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY COMPONENT TYPE: The type of component associated with a support facility','USER','dbo','TABLE','R_SF_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SUPPORT FACILITY DESCRIPTION TYPE: the type of description for a support facility, such as color, construction material etc.','USER','dbo','TABLE','R_SF_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SUPPORT FACILITY DESCRIPTION VALUE: a set of allowable codes for the description of a support facility. As these codes are dependent on the type of description, the primary key is two part.','USER','dbo','TABLE','R_SF_DESC_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE SUPPORT FACILITY DOCK TYPE: A valid type of dock used in marine operations.','USER','dbo','TABLE','R_SF_DOCK_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SUPPORT FACILITY ELECTRIC FACILITY TYPE: The type of electric facility, such as generator station or power pole.','USER','dbo','TABLE','R_SF_ELECTRIC_TYPE';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY LANDING FACILITY or HELIPORT TYPE: The type of landing facility, such as airstrip or heliport. May be a helipad (A prepared area designated and used for takeoff and landing of helicopters. (Includes touchdown or hover point.) ) or a heliport(A facility designated for operating, basing, servicing, and maintaining helicopters).','USER','dbo','TABLE','R_SF_LANDING_TYPE';
go

execute sp_addextendedproperty 'Description','MAINTAINENCE TYPE: the type of maintenace that will be done on this support facility, such as repaving, painting, surfacing etc.','USER','dbo','TABLE','R_SF_MAINTAIN_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SUPPORT FACILITY PAD TYPE: A valid type of drilling pad.','USER','dbo','TABLE','R_SF_PAD_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SUPPORT FACILITY ROAD TYPE: A valid type of road, such as public, logging, private etc.','USER','dbo','TABLE','R_SF_ROAD_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SUPPORT FACILITY STATUS: The status of the support facility, such as working, abandoned etc.','USER','dbo','TABLE','R_SF_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE SUPPORT FACILITY STATUS TYPE: The type of support facilty status, such as operational, construction status, regulatory status etc. Used to cagegorize status information.','USER','dbo','TABLE','R_SF_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SUPPORT FACILITY SURFACE TYPE: A valid list of surface types for various support facility. May include concrete, asphalt, dirt, gravel, sand etc.','USER','dbo','TABLE','R_SF_SURFACE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SUPPORT FACILITY TOWER TYPE: The type of tower, such as electrical, radio, microwave etc.','USER','dbo','TABLE','R_SF_TOWER_TYPE';
go

execute sp_addextendedproperty 'Description','VEHICLE TYPE: The type of vehicle, such as truck, car, van, minivan, motorcycle, ATV, ambulance, trailer, bus etc.','USER','dbo','TABLE','R_SF_VEHICLE_TYPE';
go

execute sp_addextendedproperty 'Description','VESSEL ROLE: The specific role played by a vessel during an operation, such as seismic source creation, drilling, cleanup, supplies transportation, personelle transportation etc.','USER','dbo','TABLE','R_SF_VESSEL_ROLE';
go

execute sp_addextendedproperty 'Description','VESSEL TYPE: The type of marine vessel, such as seismic recording, drilling rig, surveying, passenger.','USER','dbo','TABLE','R_SF_VESSEL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SUPPORT FACILITY CROSS REFERENCE TYPE: A type of relationship that exists between two support facilities. For examples, two roads may cross, a tower may exist on a road right of way, a bridge may be on a road and so on.','USER','dbo','TABLE','R_SF_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SHOW TYPE: This table identifies the appearance of the hydrocarbons in the object being evaluated. For example, show types can include Asphaltic stain, Bleeding gas, Oil fluorescence.','USER','dbo','TABLE','R_SHOW_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SHUTIN PRESSURE TYPE: This reference table defines the type of shutin pressure being measured. For example, Tubing pressure, Casing pressure or Bottom Hole pressure.','USER','dbo','TABLE','R_SHUTIN_PRESS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SOURCE: A reference table identifying the individual, company, state or government agency that provided information. For example Digitech, Dwights, PetroData, Petroleum Information or API.','USER','dbo','TABLE','R_SOURCE';
go

execute sp_addextendedproperty 'Description','REFERENCE SOURCE ORIGIN: This table identifies the projects, software applications, documents or other sources from which data may have originated.','USER','dbo','TABLE','R_SOURCE_ORIGIN';
go

execute sp_addextendedproperty 'Description','REFERENCE SPACING UNIT TYPE: the type of spacing unit. May include drilling, producing.','USER','dbo','TABLE','R_SPACING_UNIT_TYPE';
go

execute sp_addextendedproperty 'Description','SPATIAL DESCRIPTION COMPONENT TYPE: The type of component associated with a spatial description','USER','dbo','TABLE','R_SPATIAL_DESC_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','LAND LEGAL DESCRIPTION TYPE: The type of land legal description. May be for a land right, surface restriction, pool, field, spacing unit etc.','USER','dbo','TABLE','R_SPATIAL_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','SPATIAL CROSS REFERENCE TYPE: the reason why the spatial descriptions are related. Could be a spatial overlap, a business relationship etc.','USER','dbo','TABLE','R_SPATIAL_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SPATIAL POINT VERSTION TYPE: The type of version that this location version describes. Could be an original location, agreed location, alternate location, archived location etc.','USER','dbo','TABLE','R_SP_POINT_VERSION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SPATIAL ZONE DEFINITION CROSS REFERENCE REASON: Describes the reason why spatial zone definitions have been cross referenced. The most common (but not the only) reason is a replacement of a zone definition with a new one by a regulatory agency.','USER','dbo','TABLE','R_SP_ZONE_DEFIN_XREF';
go

execute sp_addextendedproperty 'Description','REFERENCE ZONE DERIVATION METHOD: Type of log on which the definition is based, such as Borehole compensated sonic, induction electric, induction gamma ray etc.','USER','dbo','TABLE','R_SP_ZONE_DERIV';
go

execute sp_addextendedproperty 'Description','REFERENCE ZONE TYPE: The type of mineral zone definiton, such as zone definition or DRRZD (Deep rights reversion zone definition)','USER','dbo','TABLE','R_SP_ZONE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE STATUS GROUP: This reference table groups status codes together. For example status of oil producer, dual oil producer and pumping oil may be grouped together as OIL.','USER','dbo','TABLE','R_STATUS_GROUP';
go

execute sp_addextendedproperty 'Description','PHYSICAL STORE STATUS: The current status of this physical data store, such as operating, closed, destroyed etc.','USER','dbo','TABLE','R_STORE_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE STRATIGRAPHIC ACQUISITION METHOD: the method that was used to arrive at the stratigraphic analysis of data. May include Biostratigraphy, Radiometric techniques, cuttings, cores, logs, seismic etc.','USER','dbo','TABLE','R_STRAT_ACQTN_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE STRATIGRAPHIC AGE METHOD: The method used to determine the age of this stratigraphic unit, such as radiometric, fossil analysis etc.','USER','dbo','TABLE','R_STRAT_AGE_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE STRATIGRAPHIC ALIAS TYPE: The type of stratigraphic alias, such as working, vendor, regulatory etc.','USER','dbo','TABLE','R_STRAT_ALIAS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE STRATIGRAPHIC COLUMN TYPE: The type of stratigraphic column, such asbiostratigraphic, lithostratigraphic, sequence stratigraphic,proposed bore hole section, type section or regional section.','USER','dbo','TABLE','R_STRAT_COLUMN_TYPE';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC COLUMN CROSS REFERENCE TYPE: The reason why the stratigraphic columns have been cross refernced. A new column may replace on outdated version, for example.','USER','dbo','TABLE','R_STRAT_COL_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE STRATIGRAPHIC CORRELATION CRITERIA: the basis or context within which the correlation was made, such as porosity, permeability, biostratigraphic age, lithologic characteristics, log character etc.','USER','dbo','TABLE','R_STRAT_CORR_CRITERIA';
go

execute sp_addextendedproperty 'Description','REFERENCE STRATIGRAPHIC CORRELATION TYPE: the type of stratigraphic correlation, such as lithostratigraphic, biostratigraphic, chronostratigraphic or other.','USER','dbo','TABLE','R_STRAT_CORR_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE STRATIGRAPHIC DESCRIPTION TYPE: the type of descriptive information included, usually classified as found in a stratigraphic lexicon. Categories may include Lithologic Characteristics, Thickness and Distribution, Relationships to Other Units, History etc.','USER','dbo','TABLE','R_STRAT_DESC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE STRATIGRAPHIC EQUIVALENCE DIRECTIONALITY: the direction in which the equivalence is valid, such as one way or two way. A lithstratigraphic bed may be equivalent to a formation, but the formation is not necessarily equivalent to the bed.','USER','dbo','TABLE','R_STRAT_EQUIV_DIRECT';
go

execute sp_addextendedproperty 'Description','REFERENCE STRATIGRAPHIC EQUIVALENCE TYPE: the type of relationship between two units, two surfaces, or a unit and surface based on an interpretation that the two strat elements are the same age (equivalent stratigraphically and/or geochronologically), although they are seperated in space.','USER','dbo','TABLE','R_STRAT_EQUIV_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE STRATIGRAPHIC FIELD STATION NODE LOCATION TYPE: the type of location for a field station node. Locations may exist across the surface of a field station or vertically, to describe depths. Types may include sample point, corner, hole surface,hole bottom etc.','USER','dbo','TABLE','R_STRAT_FLD_NODE_LOC';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC HIERARCHY TYPE: The type of hierarchy that is defined, such as biostratigraphic (Era, Period, Series...) lithostratigraphic etc.','USER','dbo','TABLE','R_STRAT_HIERARCHY';
go

execute sp_addextendedproperty 'Description','REFERENCE STRATIGRAPHIC INTERPRETATION METHOD: Interpretation method - surface sample, sub-surface, logs, seismic, etc.','USER','dbo','TABLE','R_STRAT_INTERP_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE STRATIGRAPHIC NAME SET TYPE: the type of name set, such as vendor, scientific, working, corporate, lexicon etc.','USER','dbo','TABLE','R_STRAT_NAME_SET_TYPE';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC OCCURRENCE TYPE: Indicates how a strat unit occurs relative to another strat unit. Examples may be contained, interfinger, etc.','USER','dbo','TABLE','R_STRAT_OCCURRENCE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE STRATIGRAPHIC STATUS TYPE: the status of the stratigraphic unit, such as whether the strat unit is currently in use, obsolete, replaced etc.','USER','dbo','TABLE','R_STRAT_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE STRATIGRAPHIC TOPOLOGICAL RELATIONSHIP TYPE: The type of topological relationship between two stratigraphic units, such as bounded by, adjacent to, interfingered within, overlies, underlies etc.','USER','dbo','TABLE','R_STRAT_TOPO_RELATION';
go

execute sp_addextendedproperty 'Description','REFERENCE STRATIGRAPHY TYPE: The category of stratigraphy that the STRAT UNIT is described within, such as lithostratigraphic, chronostratigraphic, biostratigraphic, radiometric etc.','USER','dbo','TABLE','R_STRAT_TYPE';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC UNIT COMPONENT TYPE: The type of component associated with a stratigraphical unit','USER','dbo','TABLE','R_STRAT_UNIT_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE STRATIGRAPHIC UNIT DESCRIPTION: any descriptive charateristic of the stratigraphic unit, such as the color or texture that may be used to identify the stratigraphic unit. Likely to be replaced in future work.','USER','dbo','TABLE','R_STRAT_UNIT_DESC';
go

execute sp_addextendedproperty 'Description','REFERENCE STRAT UNIT QUALIFIER: A qualifier that describes where on a stratigraphic unit an event is described, such as for a land right description, which may be granted to the TOP or BASE of a stratigraphic unit.','USER','dbo','TABLE','R_STRAT_UNIT_QUALIFIER';
go

execute sp_addextendedproperty 'Description','REFERENCE STRATIGRAPHIC UNIT TYPE: The type of stratigraphic unit, often described in terms of its position in a hierarchical scale, such as Eon, Epoch, Bed, Formation etc.','USER','dbo','TABLE','R_STRAT_UNIT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE STREAMER COMPONENT TYPE: The type of streamer component, such as hydrophone, depth reading, stretch section etc.','USER','dbo','TABLE','R_STREAMER_COMP';
go

execute sp_addextendedproperty 'Description','STREAMER POSITION: The position of the streamer in the array, such as surface, sea floor etc.','USER','dbo','TABLE','R_STREAMER_POSITION';
go

execute sp_addextendedproperty 'Description','REFERENCE STUDY TYPE: The type of study that is described, such as organic geochemistry, maceral analysis, paleontological analysis, benchmark environmental study, etc','USER','dbo','TABLE','R_STUDY_TYPE';
go

execute sp_addextendedproperty 'Description','SUBSTANCE COMPONENT TYPE: the type of relationship between the substance and its composite parts. For example, a sub-substance might be a chemical fraction of the parent, or a product that is created duing processing','USER','dbo','TABLE','R_SUBSTANCE_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','SUBSTANCE PROPERTY TYPE: This table contains a list of valid properties that may be defined in SUBSTANCE PROPERTY DETAIL. This table is parent to the control column, which is used to control the behavior of properties that are entered into SUBSTANCE PROPERTY TYPE. Care should be taken in populating and managing this table, taking into account the VERTICAL TABLE procedure recommendations in the PPDM WIKI.','USER','dbo','TABLE','R_SUBSTANCE_PROPERTY';
go

execute sp_addextendedproperty 'Description','SUBSTANCE USE RULE: A rule that describes in more detail a rule controlling when or how this substance definition should be used.','USER','dbo','TABLE','R_SUBSTANCE_USE_RULE';
go

execute sp_addextendedproperty 'Description','SUBSTANCE CROSS REFERENCE TYPE: the type of relationship between two substances, other than compositional relationships.','USER','dbo','TABLE','R_SUBSTANCE_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SOFTWARE BUSINESS ASSOCIATE ROLE: The role that a business associate has in a software application. May include the vendor who created it, the vendor support resources, internal support resources etc.','USER','dbo','TABLE','R_SW_APP_BA_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE SOFTWARE APPLICATION FUNCTION: A list of valid functions that a software application may have. Includes word processing, calculations, geologic interpretation, accounting, production accounting etc.','USER','dbo','TABLE','R_SW_APP_FUNCTION';
go

execute sp_addextendedproperty 'Description','REFERENCE SOFTWARE APPLICATION TYPE:  Use this table to classify the type of function that an application used.  Effective if applications should be grouped into collections (such as all back ofice applications, or all drilling applications)','USER','dbo','TABLE','R_SW_APP_FUNCTION_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SOFTWARE APPLICATION CROSS REFERENCE: Use this column to indicate the reason why you have to cross referenced applications to each other. This is useful to keep track of software products that replace others, or products that provide a data input to another application, or accept an input from another. You can also use it to indicate dependencies in workflows (which application is used before, after or in conjunction with anothe','USER','dbo','TABLE','R_SW_APP_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE TAX CREDIT CODE: Code indicating the well qualifies for a tax credit. "C" = credit for the well being permitted for coalbed methane gas.','USER','dbo','TABLE','R_TAX_CREDIT_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE TEST EQUIPMENT: This reference table describes specific types of equipment used for well testing. For example casing packer, hook wall packer or straddle packer.','USER','dbo','TABLE','R_TEST_EQUIPMENT';
go

execute sp_addextendedproperty 'Description','REFERENCE TEST PERIOD TYPE: This reference table describes a well test time pressure period type. For example hydrostatic, shutin, valve open or flowing.','USER','dbo','TABLE','R_TEST_PERIOD_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE TEST RECOVERY METHOD: This reference tables describes method by which fluid was recovered for a well test. For example pipe or chamber.','USER','dbo','TABLE','R_TEST_RECOV_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE TEST RESULT: This reference table describes, in general terms, the final result of a well test. For example successful, misrun, pipe failure, packer failure or tester plugged.','USER','dbo','TABLE','R_TEST_RESULT';
go

execute sp_addextendedproperty 'Description','SHUTOFF TYPE: Code identifying the type of shutoff used in the wellbore (e.g., bridge plug, cased off, plugged off, or squeezed etc.).','USER','dbo','TABLE','R_TEST_SHUTOFF_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL TEST SUBTYPE: This reference tables provides a detailed description of the specific type of well test which is performed on the well. For example casing packer, hook wall packer or straddle packer.','USER','dbo','TABLE','R_TEST_SUBTYPE';
go

execute sp_addextendedproperty 'Description','R TIMEZONE: a valid list of time zones. You can obtain a list of timezones from www.worldtimezone.com.','USER','dbo','TABLE','R_TIMEZONE';
go

execute sp_addextendedproperty 'Description','R TITLE OWNERSHIP TYPE: used to refer to type of ownership for titles only. May be life estate holder, joint tenant, tentan in common...','USER','dbo','TABLE','R_TITLE_OWN_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE TOUR OCCURENCE TYPE: This reference table identifies a type of well activity that can be described as a well tour occurence. For example blowout or lost circulation.','USER','dbo','TABLE','R_TOUR_OCCURRENCE_TYPE';
go

execute sp_addextendedproperty 'Description','HEADER FORMAT: the header format used by the trace data, such as IEEE float, IBM float, 32 bit, 16 bit etc.','USER','dbo','TABLE','R_TRACE_HEADER_FORMAT';
go

execute sp_addextendedproperty 'Description','TRACE HEADER WORD: list of allowed trace header words for seismc trace data.','USER','dbo','TABLE','R_TRACE_HEADER_WORD';
go

execute sp_addextendedproperty 'Description','TRANSACTION COMPONENT TYPE: the type of component that is associated with this transaction. Could be a seismic set that was sold, the jurisdiction in which the transaction occured, the business associate who purchased the data, a broker who negotiated the transaction, the contract that governed the transaction etc. Could include sent to client, billing, used for inspection, project control etc.','USER','dbo','TABLE','R_TRANS_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','TRANSACTION STATUS: The status of this transaction, such as complete and paid, pending approval, approved, cancelled etc.','USER','dbo','TABLE','R_TRANS_STATUS';
go

execute sp_addextendedproperty 'Description','SEISMIC TRANSACTION TYPE: The type of seismc transaction that has been arranged, such as sale, trade, lease etc.','USER','dbo','TABLE','R_TRANS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE TREATMENT FLUID: A reference table identifying the type of treating fluid used in the treatment operation of the well. For example, Oil, Water, Acid.','USER','dbo','TABLE','R_TREATMENT_FLUID';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL TREATMENT TYPE: This reference table identifies the type of treatment job performed on the well. For example, hydraulic fracturing, acidizing, nitroglycerine explosives etc.','USER','dbo','TABLE','R_TREATMENT_TYPE';
go

execute sp_addextendedproperty 'Description','TUBING GRADE: the tensile strength of the tubing material. A system of classifying the material specifications for steel alloys used in the manufacture of tubing.','USER','dbo','TABLE','R_TUBING_GRADE';
go

execute sp_addextendedproperty 'Description','REFERENCE TUBING TYPE: This reference table describes the particular type of tubular or type. For example tubing, casing or liner. This is a general classification. A more specific description can be found in R_LINER_TYPE.','USER','dbo','TABLE','R_TUBING_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE TVD METHOD: This reference table defines the method used to determine the true vertical depth.','USER','dbo','TABLE','R_TVD_METHOD';
go

execute sp_addextendedproperty 'Description','VALUE QUALITY: This table is used for the quality of data in a row, usually with reference to the method or procedures used to load the data, although other types of quality reference are permitted.','USER','dbo','TABLE','R_VALUE_QUALITY';
go

execute sp_addextendedproperty 'Description','REFERENCE VELOCITY COMPUTATION METHOD: A reference table identifying methods for computing seismic velocity. For example, checkshot survey, interpolation...','USER','dbo','TABLE','R_VELOCITY_COMPUTE';
go

execute sp_addextendedproperty 'Description','REFERENCE VELOCITY DIMENSION: The dimensions of the velocites that have been collected, usually point or interval velocities.','USER','dbo','TABLE','R_VELOCITY_DIMENSION';
go

execute sp_addextendedproperty 'Description','REFERENCE VELOCITY TYPE: A reference table identifying the valid types of seismic velocity. Horizontal velocities and vertical velocites are typical types.','USER','dbo','TABLE','R_VELOCITY_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE VERTICAL DATUM TYPE: A reference table identifying valid types of Vertical Datums. For example, geoidal height the height above the geoid, elevation the height above mean sea level.','USER','dbo','TABLE','R_VERTICAL_DATUM_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE POINT: The point to which the offsets are referenced. In many cases, this is the primary antenna, but in some cases other positions on the vessel are used as the reference point.','USER','dbo','TABLE','R_VESSEL_REFERENCE';
go

execute sp_addextendedproperty 'Description','VESSEL SIZE:  A valid reference to the size of the vessel.  Do not put dimensions or displacement in this table.  This refers to the class of vessel, such as those found here http://en.wikipedia.org/wiki/Category:Ships_by_type','USER','dbo','TABLE','R_VESSEL_SIZE';
go

execute sp_addextendedproperty 'Description','VOLUME FRACTION: This table is used to indicate the type of oil that was separated via the fractional distillation.','USER','dbo','TABLE','R_VOLUME_FRACTION';
go

execute sp_addextendedproperty 'Description','REFERENCE VOLUME METHOD: A reference table identifying the type of method used to determine the volume of fluids moved. Examples would be measured, prorated, engineering study, etc.','USER','dbo','TABLE','R_VOLUME_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE VERTICAL SEISMIC PROFILE TYPE: A reference table identifying valid types of VSP. For example, upgoing, downgoing, ...','USER','dbo','TABLE','R_VSP_TYPE';
go

execute sp_addextendedproperty 'Description','WASTE ADJUSTMENT REASON: The reason for an adjustemnet between the shipped and received volume, such as temperature based shrinkage or expansion, spillage, evaporation etc.','USER','dbo','TABLE','R_WASTE_ADJUST_REASON';
go

execute sp_addextendedproperty 'Description','WASTE FACILITY TYPE: The type of waste handling facility, such as a pit, incinerator etc.','USER','dbo','TABLE','R_WASTE_FACILITY_TYPE';
go

execute sp_addextendedproperty 'Description','WASTE HANDLING METHOD: The method used to handle disposal of the waste material, such as incineration, neutralization, burying etc.','USER','dbo','TABLE','R_WASTE_HANDLING';
go

execute sp_addextendedproperty 'Description','WASTE HAZARD: The hazardous nature of the material, such as dangerous goods.','USER','dbo','TABLE','R_WASTE_HAZARD_TYPE';
go

execute sp_addextendedproperty 'Description','WASTE MATERIAL: The material that has been shipped as waste, such as drill mud.','USER','dbo','TABLE','R_WASTE_MATERIAL';
go

execute sp_addextendedproperty 'Description','WASTE ORIGIN TYPE: The type of location that this waste originated from, such as facility or well.','USER','dbo','TABLE','R_WASTE_ORIGIN';
go

execute sp_addextendedproperty 'Description','REFERENCE WATER BOTTOM ZONE: A reference table identifying valid water bottom zones. This code is retained in Louisiana as special allowable area or zone.','USER','dbo','TABLE','R_WATER_BOTTOM_ZONE';
go

execute sp_addextendedproperty 'Description','REFERENCE WATER CONDITION TYPE: A list of valid conditions of a large water body, such as an ocean, sea, gulf or lake. Could include values such as choppy, high swell, rough.','USER','dbo','TABLE','R_WATER_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE WATER DATUM: Reference datum to which the water depth is referenced, such as mean sea level.','USER','dbo','TABLE','R_WATER_DATUM';
go

execute sp_addextendedproperty 'Description','ANALYSIS PROPERTY VALUE CODE: the code assigned to the analysis property by observation, in cases where numeric values are not used.','USER','dbo','TABLE','R_WATER_PROPERTY_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE WEATHER CONDITION TYPE: a table listing valid kinds of weather conditions such as sunny and calm, rain showers, snow, ice fog etc.','USER','dbo','TABLE','R_WEATHER_CONDITION';
go

execute sp_addextendedproperty 'Description','CAUSE TYPE: The type of cause that resulted in a change to the activity in the well. Ofen, things that cause downtimes (cessation of production, constrained production or deferred production). Causes are usually defined hierarchically.','USER','dbo','TABLE','R_WELL_ACTIVITY_CAUSE';
go

execute sp_addextendedproperty 'Description','WELL ACTIVITY COMPONENT TYPE: The type of component associated with a well activity','USER','dbo','TABLE','R_WELL_ACTIVITY_COMP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL ACTIVITY TYPE EQUIVALENCE: This value can be used to qualify the equivalence measure, so that you can set up equivalences for activities that are closely related, or for activities that subsume or are fully equivalent with another activity type.','USER','dbo','TABLE','R_WELL_ACT_TYPE_EQUIV';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL ALIAS LOCATION TYPE: The position on the wellbore that this alias is assigned to. Common types are top hole and bottom hole.','USER','dbo','TABLE','R_WELL_ALIAS_LOCATION';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL PRESSURE CIRCULATION TYPE: Indicates whether one or both pumps were on the hole, such as Single, Combined, Parallel.','USER','dbo','TABLE','R_WELL_CIRC_PRESS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL CLASS: This reference table describes the classification of a well. This may include, but is not restricted to the Lahee classification scheme. For example development, new field wildcat or outpost.','USER','dbo','TABLE','R_WELL_CLASS';
go

execute sp_addextendedproperty 'Description','WELL COMPONENT TYPE: The type of component associated with a well','USER','dbo','TABLE','R_WELL_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL DATUM TYPE: A reference table identifying the type of point or horizontal surface used as an elevation reference for measurements in a well. Examples: kelly bushing, ground, sea level.','USER','dbo','TABLE','R_WELL_DATUM_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL DOWNTIME TYPE: The type of downtime experienced during a well operation or event. Downtime types may include downtime, constrained production, deferred production. Added to allow some granularity of describing downtime events wthout having to overload the event type table.','USER','dbo','TABLE','R_WELL_DOWNTIME_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL DRILLING OPERATIONS TYPE: The type of drilling operation that the bit is doing, such as drilling, coring or reaming.','USER','dbo','TABLE','R_WELL_DRILL_OP_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL FACILITY USE TYPE: The type of use that a facility is put to for a particular well, such as processing, pumping station, steam production etc.','USER','dbo','TABLE','R_WELL_FACILITY_USE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL LEVEL TYPE: Indicates which well component this row describes, as outlined in www.WhatIsAWell.org. Values may include WELL, WELL ORIGIN, WELLBORE, WELLBORE SEGMENT, WELLBORE COMPLETION or WELLBORE CONTACT INTERVAL.','USER','dbo','TABLE','R_WELL_LEVEL_TYPE';
go

execute sp_addextendedproperty 'Description','WELL LICENSE CONDITION TYPE: the type of condition applied to the well license, such as flaring rate, venting rate, production rate, H2S content limit, emissions etc.','USER','dbo','TABLE','R_WELL_LIC_COND';
go

execute sp_addextendedproperty 'Description','WELL LICENSE CONDITION CODE Codified values assigned to a well license condition. Can include any type of condition with the exception of values related to STRAT UNITS (explicit), Products (explicit) or values (use numeric values).','USER','dbo','TABLE','R_WELL_LIC_COND_CODE';
go

execute sp_addextendedproperty 'Description','DUE CONDITION: The state that must be achieved for the condition to become effective. For example, a report may be due 60 days after operations commence (or cease).','USER','dbo','TABLE','R_WELL_LIC_DUE_CONDITION';
go

execute sp_addextendedproperty 'Description','REFERENCE LICENSE VIOLATION RESOLUTION TYPE: The type of resolution to a violation of a license term, such as the payment of a fine or creation of new processes.','USER','dbo','TABLE','R_WELL_LIC_VIOL_RESOL';
go

execute sp_addextendedproperty 'Description','REFERENCE VIOLATION TYPE: The type of violation of a license that is being recorded. Can be as simple as failure to submit necessary reports or something more difficult such as improper procedures.','USER','dbo','TABLE','R_WELL_LIC_VIOL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL LOG CLASS: This table lists the classes of well log files that may be created.','USER','dbo','TABLE','R_WELL_LOG_CLASS';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL NODE STRAT UNIT PICK METHOD: The method used to pick the distance from a well node to the top and base of a stratigraphic unit. Could be logs, cores, other kinds of studies etc.','USER','dbo','TABLE','R_WELL_NODE_PICK_METHOD';
go

execute sp_addextendedproperty 'Description','REFERENCE WELLBORE SHAPE: A reference table describing a type of wellbore shape. For example vertical, horizontal, directional or s-shaped.','USER','dbo','TABLE','R_WELL_PROFILE_TYPE';
go

execute sp_addextendedproperty 'Description','R WELL QUALIFICATION TYPE: defines the type of method used to determine that the well is capable of producing in paying quantities. test, logs,','USER','dbo','TABLE','R_WELL_QUALIFIC_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL MISC DATA REFERENCE VALUE TYPE: In the case where a measured value must be referenced to another value (such as time, depth or an instrument setting), capture the type of reference value here.','USER','dbo','TABLE','R_WELL_REF_VALUE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL RELATIONSHIP: A reference table describing the type of relationship a well/wellbore may have with a parent well/wellbore. For example sidetracked, recompleted or deepening.','USER','dbo','TABLE','R_WELL_RELATIONSHIP';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL SERVICE METRIC: Describes the types of metrics that are captured for each service provided on a well, as reported by reporting period (shift, day, tour etc.). Metrics such as hours, volumes pumped, distance completed etc can be captured. Actual costs should be reported using the FINANCE module.','USER','dbo','TABLE','R_WELL_SERVICE_METRIC';
go

execute sp_addextendedproperty 'Description','WELL SERVICE METRIC CODE: Validated text that represents the value for a metric, such as started, finished or quality of service. Codes are created with reference to the type of metric that is being captured.','USER','dbo','TABLE','R_WELL_SERV_METRIC_CODE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL SET ROLE: The role that this well record supports in the wellset. This may include values such as service, relief, original skidded, abandoned, planned, not drilled etc.','USER','dbo','TABLE','R_WELL_SET_ROLE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL SET TYPE: The kind of well set that is described. Mainly used to group all of the components in a well through the life cycle, so that all the well objects from planning to disposal (whether or not successful or even physically created) can begathered together.','USER','dbo','TABLE','R_WELL_SET_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL SUPPORT FACILITY USE TYPE: The type of use that a support facility is put to for a particular well, such as access, drilling, communications etc.','USER','dbo','TABLE','R_WELL_SF_USE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL STATUS: This reference table defines the status of the well.','USER','dbo','TABLE','R_WELL_STATUS';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL STATUS QUALIFIER: Use this reference table to track the valid qualifiers for each of the well status facets created by the PPDM workgroup for Well status and classification (http://www.ppdm.org/standards/wellstatus). This table is only used for facets that have qualifiers, and connects the proper qualifiers to the status types. Qualilfier values are managed in R WELL STATUS QUAL VALUE.','USER','dbo','TABLE','R_WELL_STATUS_QUAL';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL STATUS QUALIFIER VALUE: This reference table contains the valid values for each of the well status qualifiers defined by the well status workgroup http://www.ppdm.org/standards/wellstatus. This table should only be used to store the values for qualifiers, and not for well status values themselves. For example, in the well status facet FLUID TYPE, a valid qualifier is ABUNDENCE (this value goes in R_WELL_STATUS_QUAL). This table will capture the values PRIMARY, SECONDARY, SHOW and TRACE.','USER','dbo','TABLE','R_WELL_STATUS_QUAL_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL STATUS PLOT SYMBOL:  This table is used to associate each plot symbol with the appropriate facet definition or definitions that contribute to its construction.  As described in Well Status http://www.ppdm.org/standards/wellstatus, the plot symbols may be based on one or more facet definitions that are used IN COMBINATION to create a symbol.  The column FACET_COUNT is used to state how many facets are used in the creation of each plot symbol.','USER','dbo','TABLE','R_WELL_STATUS_SYMBOL';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL STATUS TYPE: The type of status reported for the well. These should include the facet values from Well Status http://www.ppdm.org/standards/wellstatus. Each facet type is one row in this table. As needed, statuses that are reported from various agencies may also be included.','USER','dbo','TABLE','R_WELL_STATUS_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL STATUS CROSS-REFERENCE(PLOT SYMBOL): This table is used to cross-reference Well Statuses. As described in Well Status http://www.ppdm.org/standards/wellstatus, the plot symbols may be based on one or more facet definitions that are used IN COMBINATION to create a symbol.','USER','dbo','TABLE','R_WELL_STATUS_XREF';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL TEST TYPE: This reference table identifies the general type of test used to evaluate the potential of the well. For example, Drill Stem Tests (DST), Repeat Formation Tests (RFT), Initial Potential(IP).','USER','dbo','TABLE','R_WELL_TEST_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL CROSS REFERENCE TYPE: The type of cross reference between two wells. This may include relationships between well and well bore (also handled more simply by PARENT UWI) or planned and actual wells. Functional relationships may also be captured if desired.','USER','dbo','TABLE','R_WELL_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','WELL ZONE INTERVAL VALUE TYPE: The type of value that is associated with this well zone interval. Note that where possible, it is preferable to use explicit tables - by choice, iff possible, this table should only be used when no other table can be found.','USER','dbo','TABLE','R_WELL_ZONE_INT_VALUE';
go

execute sp_addextendedproperty 'Description','REFERENCE WIND STRENGTH: The strength of the wind, often measured according to a standard list of wind strengths, such as the Beaufort Wind Scale (www.bom.gov.au/lam/glossary/beaufort.shtml)','USER','dbo','TABLE','R_WIND_STRENGTH';
go

execute sp_addextendedproperty 'Description','LAND WORK BID TYPE: The type of bid component that is part of a work bid. Examples may include drilling, shooting seismic etc.','USER','dbo','TABLE','R_WORK_BID_TYPE';
go

execute sp_addextendedproperty 'Description','WORK ORDER BUSINESS ASSOCIATE ROLE: A list of valid roles played by business associates with respect to carrying out instructions for a work order. Roles include shipping address, client etc.','USER','dbo','TABLE','R_WO_BA_ROLE';
go

execute sp_addextendedproperty 'Description','WORK ORDER COMPONENT TYPE: The type of component that has been associated with the work order, such as governing contract, seismic associated, projects associated etc. Additional FKs may be added to this table. Associations with obligations (related to payments etc) are captured in OBLIGATION COMPONENT.','USER','dbo','TABLE','R_WO_COMPONENT_TYPE';
go

execute sp_addextendedproperty 'Description','WORK ORDER CONDITION: Lists the pre-conditions that must exist in order for this work order to be completed or filled. Can indicate a need for approval, payment, for a facility to be taken off-line (for maintenance), for notification to be sent out etc.','USER','dbo','TABLE','R_WO_CONDITION';
go

execute sp_addextendedproperty 'Description','WORK ORDER DELIVERY TYPE: The type of delivery, such as received product, sent product, returned products etc.','USER','dbo','TABLE','R_WO_DELIVERY_TYPE';
go

execute sp_addextendedproperty 'Description','WORK ORDER INSTRUCTION: A list of coded values for specific instructions associated with work orders. May be used in conjunction with description fields as appropriate.','USER','dbo','TABLE','R_WO_INSTRUCTION';
go

execute sp_addextendedproperty 'Description','WORK ORDER TYPE: the type of work order, such as data circulation,flowing hole remediation, brokerage etc.','USER','dbo','TABLE','R_WO_TYPE';
go

execute sp_addextendedproperty 'Description','WORK ORDER CROSS REFERENCE TYPE: the type of relationships between work orders. These relationships may be historical (work order replaces another), functional (this work order was divided into two subordinate work orders) or transactional (company A sent a work order which was used to create a new work order).','USER','dbo','TABLE','R_WO_XREF_TYPE';
go

execute sp_addextendedproperty 'Description','SAMPLE: The sample table is used to store the physical properties and the most primary information about the sample. A row in this table may be a complete initial sample or a portion of that sample that is used for analysis. A sample may be collected during onshore or offshore well drilling operations, from a stratigraphic field station (i.e., outcrop), measured section or from any other location. These samples are used in a variety of technical analysis, including chemical, biostratigraphic, geochemical and radiometric.','USER','dbo','TABLE','SAMPLE';
go

execute sp_addextendedproperty 'Description','SAMPLE ALIAS: Use this table to capture all names, codes and identifiers that are assigned to a sample, by a lab, agency, study or other source. Use this table to identify the system ID assigned to a sample by another application; this will allow you to query between systems.','USER','dbo','TABLE','SAMPLE_ALIAS';
go

execute sp_addextendedproperty 'Description','SAMPLE COMPONENT: This table is used to store secondary type information with respect to the sample. This information is regarding the factors surrounding the sample. This can be anything from the land the sample was extracted from to the actual physical location where the sample is being stored.','USER','dbo','TABLE','SAMPLE_COMPONENT';
go

execute sp_addextendedproperty 'Description','SAMPLE DESCRIPTION OTHER: This table is to be used to capture descriptions of the samples that are not covered in SAMPLE LITH DESC. It is generally preferable to use SAMPLE LITH DESC when possible, as it provides better data quality management mechanisms.','USER','dbo','TABLE','SAMPLE_DESC_OTHER';
go

execute sp_addextendedproperty 'Description','SAMPLE LITHOLOGIC DESCRIPTION: This table is used to describe the characteristics of a rock sample such as color, permeability, porosity, rock type and the rock matrix. Values not supported in this table and for other sample types may be loaded in SAMPLE DESC OTHER using a vertical structure.','USER','dbo','TABLE','SAMPLE_LITH_DESC';
go

execute sp_addextendedproperty 'Description','SAMPLE ORIGIN: This table is used to describe where the sample was collected from. A single sample may be the result of collections at several positions or of several samples. Use this table to indicate the particular well, land right, core type, etc that the sample came from. This table also keeps track of the quantity used from the sample for the particular analysis.','USER','dbo','TABLE','SAMPLE_ORIGIN';
go

execute sp_addextendedproperty 'Description','SEISMIC THREE DIMENSIONAL DATA: A three dimensional set of data. The data set may be the entire set as originally acquired, a subset of data that has been processed, interpreted or sold, or a set derived through combining other data sets.','USER','dbo','TABLE','SEIS_3D';
go

execute sp_addextendedproperty 'Description','SEISMIC ACQUISITION DESIGN: Stores field parmeters used during field acquisition for a seismic segment or group of segments.','USER','dbo','TABLE','SEIS_ACQTN_DESIGN';
go

execute sp_addextendedproperty 'Description','SEISMIC ACQISITION SPECTRUM: this table is used to list all the additional data types that are acquired during this recording. Can include gravity, magnetics etc.','USER','dbo','TABLE','SEIS_ACQTN_SPECTRUM';
go

execute sp_addextendedproperty 'Description','SEISMIC ACQUISITION SURVEY: a group of lines created for the purpose of seismic data acquisition, either shooting or purchase.','USER','dbo','TABLE','SEIS_ACQTN_SURVEY';
go

execute sp_addextendedproperty 'Description','SEISMIC SET ACTIVITY: Use this table to track activities on a seismic set over its life time, from planning through acquisition, processing, interpretation, cleanup and divestment.','USER','dbo','TABLE','SEIS_ACTIVITY';
go

execute sp_addextendedproperty 'Description','SEISMIC SET ALIAS: Alternate name for the seismic set.','USER','dbo','TABLE','SEIS_ALIAS';
go

execute sp_addextendedproperty 'Description','SEISMIC BUSINESS ASSOCIATE SERVICE: this table can be used to track the business associates with whom you do business for seismic data. At present, you can track who wasn involved with seismic acquisition - roles and relevant dates can be tracked.','USER','dbo','TABLE','SEIS_BA_SERVICE';
go

execute sp_addextendedproperty 'Description','SEIS BIN GRID: a theoretically defined matrix used to group data points','USER','dbo','TABLE','SEIS_BIN_GRID';
go

execute sp_addextendedproperty 'Description','SEISMIC BIN ORIGIN: Where all or part of a previously created set of bins is used as input into a new bin grid, the bin set (and portion of the bin set) that was used as input is defined in this table.','USER','dbo','TABLE','SEIS_BIN_ORIGIN';
go

execute sp_addextendedproperty 'Description','SEIS BIN OUTLINE: This table may be used to capture outlines for bin grids that are referenced to the inline and cross line coordinates. If geographic latitude and longitude values are captured, the SPATIAL DESCRIPTION module should be used to capture a mapping outline. UTM or local coordinates may be stored here.','USER','dbo','TABLE','SEIS_BIN_OUTLINE';
go

execute sp_addextendedproperty 'Description','SEIS BIN POINT: information about the points in a bin grid. The coordinate system used is inferred to be the same as defined in SEIS BIN GRID. This entity may be only used where bin grids are irregular, so that compression is not practical or desirable','USER','dbo','TABLE','SEIS_BIN_POINT';
go

execute sp_addextendedproperty 'Description','SEIS BIN POINT TRACE : information about the traces associated with points in a bin grid.','USER','dbo','TABLE','SEIS_BIN_POINT_TRACE';
go

execute sp_addextendedproperty 'Description','SEISMIC BIN POINT VERSION: alternate location information for a surface or subsurface point associated with a seismic segment. May be a shot, receiver, surveyed etc position. The versions may represent coordinates in an alternate geodetic reference,or historical locations.','USER','dbo','TABLE','SEIS_BIN_POINT_VERSION';
go

execute sp_addextendedproperty 'Description','SEISMIC CHANNEL: Information describing the recorded results on a single channel of a seismic record. This level of detail should be captured only when there is a business driver to do so, large table sizes should be expected. Some implementors may wish to only capture details about anomylous records in this way.','USER','dbo','TABLE','SEIS_CHANNEL';
go

execute sp_addextendedproperty 'Description','SEISMIC GROUP COMPONENT: This table describes the grouping of seismic sets into other sets. Examples include acquisition data grouped into a processing set, processed sets grouped into an interpretation set, acquisition sets grouped into a seismic survey etc.','USER','dbo','TABLE','SEIS_GROUP_COMP';
go

execute sp_addextendedproperty 'Description','SEISMIC SET INSEPCTION: This table may be used to track quality inspections on data sale offerings.','USER','dbo','TABLE','SEIS_INSPECTION';
go

execute sp_addextendedproperty 'Description','SEISMIC INSPECTION COMPONENET: Use this table to list the sections, tapes and other data that was presented during a seismic inspection. In some cases, the data that is inspected does not correspond with the data that is later involved in a transaction.','USER','dbo','TABLE','SEIS_INSP_COMPONENT';
go

execute sp_addextendedproperty 'Description','SEIS SET INTERPRETATION COMPONENT: which objects were part of the inspection. Note that in some cases, the products that are inspected are not the same as those that are eventually sold.','USER','dbo','TABLE','SEIS_INTERP_COMP';
go

execute sp_addextendedproperty 'Description','SEISMIC INTERPRETATION LOAD: technical data modifications, such as amplitude gains, that are made during the load into an interpretation system.','USER','dbo','TABLE','SEIS_INTERP_LOAD';
go

execute sp_addextendedproperty 'Description','SEISMIC INTERPRETATION LOAD PARAMETER: Describes the parameters applied during each step of the load process into an interpretation system.','USER','dbo','TABLE','SEIS_INTERP_LOAD_PARM';
go

execute sp_addextendedproperty 'Description','SEISMIC INTERPRETED SET: A set of seismic data that has been grouped for interpretation.','USER','dbo','TABLE','SEIS_INTERP_SET';
go

execute sp_addextendedproperty 'Description','INTERPRETED SURFACE: the surface which has been interpreted.','USER','dbo','TABLE','SEIS_INTERP_SURFACE';
go

execute sp_addextendedproperty 'Description','SEISMIC LICENSE: An approval or authorization to conduct operations related to geophysical or seismic data, must be associated with a SEIS SET (at the early stages could be a planned seismic set).','USER','dbo','TABLE','SEIS_LICENSE';
go

execute sp_addextendedproperty 'Description','SEISMIC LICENSE NAME ALIAS: The Name Alias table stores multiple alias names for a given license.','USER','dbo','TABLE','SEIS_LICENSE_ALIAS';
go

execute sp_addextendedproperty 'Description','SEISMIC LICENSE AREA: this table tracks the areas into which a seismic license falls.','USER','dbo','TABLE','SEIS_LICENSE_AREA';
go

execute sp_addextendedproperty 'Description','SEISMIC LICENSE CONDITION: A list of conditions that are applied to a seismic or geophysical license. Could include stipulations about disposal, timber salvage, inspections, compliance with legislation etc.','USER','dbo','TABLE','SEIS_LICENSE_COND';
go

execute sp_addextendedproperty 'Description','SEISMIC LICENSE REMARK: a text description to record general comments on the license tracking when remark was made, who is the author and the type of remark.','USER','dbo','TABLE','SEIS_LICENSE_REMARK';
go

execute sp_addextendedproperty 'Description','SEISMIC LICENSE STATUS: Tracks the status of a license throughout its lifetime. Various types of status may be included at the discretion of the implementor.','USER','dbo','TABLE','SEIS_LICENSE_STATUS';
go

execute sp_addextendedproperty 'Description','SEISMIC LICENSE TYPE: The type of geophysical license that has been granted. In some jurisdicitons a single license may be granted to cover all operations, in others seperate licenses are granted based on the type of operation.','USER','dbo','TABLE','SEIS_LICENSE_TYPE';
go

execute sp_addextendedproperty 'Description','SEISMIC LICENSE VIOLATION: Use this table to track incidents where the terms of a license have been violated (or perhaps are claimed to be violated). At this time the table is relatively simple in content.','USER','dbo','TABLE','SEIS_LICENSE_VIOLATION';
go

execute sp_addextendedproperty 'Description','SEISMIC LINE A valid set of shot and receiver locations acquired together and which are initially intended to be processed together.','USER','dbo','TABLE','SEIS_LINE';
go

execute sp_addextendedproperty 'Description','SEISMIC PATCH: This table is used to capture generalized recording patch designs. Details about the patch design are contained in the table SEIS_PATCH_DESC or as a digital file that is stored in the RM module.','USER','dbo','TABLE','SEIS_PATCH';
go

execute sp_addextendedproperty 'Description','SEISMIC PATCH DESCRIPTION: Details about the patch design used, such as offset details. Each contiguous linear component of the patch layout is described as a row in this table. A 2D split spread layout is described using two rows in this table. A 3D patch may require many rows.','USER','dbo','TABLE','SEIS_PATCH_DESC';
go

execute sp_addextendedproperty 'Description','SEISMIC PICK:  Use this table to track all of the picks made in seismic interpretation applications.  Mark versions, preferred picks and more.','USER','dbo','TABLE','SEIS_PICK';
go

execute sp_addextendedproperty 'Description','SEISMIC POINT: a surface or subsurface point associated with a seismi c segment. May be a shot, receiver, surveyed etc position.','USER','dbo','TABLE','SEIS_POINT';
go

execute sp_addextendedproperty 'Description','SEISMIC POINT FLOW: Identifies a point that has begun to flow from a sub surface aquifer. These positions require environmetal remediation.','USER','dbo','TABLE','SEIS_POINT_FLOW';
go

execute sp_addextendedproperty 'Description','SEISMIC POINT FLOW DESCRIPTION: descriptive details about a flowing hole.','USER','dbo','TABLE','SEIS_POINT_FLOW_DESC';
go

execute sp_addextendedproperty 'Description','SEISMIC POINT SUMMARY: Tracks summary information about sections of seismic sets, including the first and last positions on the set, the area of the section and its coverage.','USER','dbo','TABLE','SEIS_POINT_SUMMARY';
go

execute sp_addextendedproperty 'Description','SEISMIC POINT VERSION: alternate location information for a surface or subsurface point associated with a seismic segment. May be a shot, receiver, surveyed etc position. The versions may represent coordinates in an alternate geodetic reference, or historical locations.','USER','dbo','TABLE','SEIS_POINT_VERSION';
go

execute sp_addextendedproperty 'Description','SEISMIC PROCESSING COMPONENT: this table keeps track of all the inputs to and outputs from the processing process.','USER','dbo','TABLE','SEIS_PROC_COMPONENT';
go

execute sp_addextendedproperty 'Description','SEISMIC PROCESSING PARAMETER: this table lists the processing parameters applied at each step of a processing flow. The vertical nature of this table allows most types of parameters to be captured.','USER','dbo','TABLE','SEIS_PROC_PARM';
go

execute sp_addextendedproperty 'Description','SEISMIC PROCESSING PLAN: this table is used to capture the intended processing flow, called the processing plan. Some organizations have a small number of standardized plans which are applied to various seismic processing sets as appropriate.','USER','dbo','TABLE','SEIS_PROC_PLAN';
go

execute sp_addextendedproperty 'Description','SEISMIC PROCESSING PLAN PARAMETERS: This table provides detail about the proposed processing plan to the level to parameter settings that are to be applied.','USER','dbo','TABLE','SEIS_PROC_PLAN_PARM';
go

execute sp_addextendedproperty 'Description','SEISMIC PROCESSING PLAN STEP: This table lists the processing steps that are proposed for a processing plan and the order in which they should be applied.','USER','dbo','TABLE','SEIS_PROC_PLAN_STEP';
go

execute sp_addextendedproperty 'Description','SEISMIC PROCESSING SET: a set of data that is processed as a single unit.','USER','dbo','TABLE','SEIS_PROC_SET';
go

execute sp_addextendedproperty 'Description','SEISMIC PROCESSING STEP: A step in the processing of seismic trace data. Steps may be generic (such as demultiplexing), or specific (such as a particular program or algorithm for flattening data).','USER','dbo','TABLE','SEIS_PROC_STEP';
go

execute sp_addextendedproperty 'Description','SEISMIC PROCESING STEP COMPONENT: This table allows inputs and outputs from a processing flow to be associated with individual processing steps, in the event this level of detail is desirable.','USER','dbo','TABLE','SEIS_PROC_STEP_COMPONENT';
go

execute sp_addextendedproperty 'Description','SEISMIC RECORD: this table is used to capture details about seismic records. It is not strictly necessary for an actual record to exist on a tape to use this table, although it is preferable.','USER','dbo','TABLE','SEIS_RECORD';
go

execute sp_addextendedproperty 'Description','SEISMIC RECEIVER INSTRUMENTS MAKE: make and model of the equipment used.','USER','dbo','TABLE','SEIS_RECVR_MAKE';
go

execute sp_addextendedproperty 'Description','SEISMIC RECEIVER SETUP: Describes the nominal set up of the seismic receiver array design.','USER','dbo','TABLE','SEIS_RECVR_SETUP';
go

execute sp_addextendedproperty 'Description','SEISMIC SEGMENT: A portion of a seismic line created or used for some business process, such as processing, transactions, interpretation, acquisition etc.','USER','dbo','TABLE','SEIS_SEGMENT';
go

execute sp_addextendedproperty 'Description','SEISMIC SET: A seismic set is a super type of various types of seismic collections. Valid types of seismic sets include SEIS_LINE, SEIS_3D, SEIS_SEGMENT and SEIS_SURVEY.','USER','dbo','TABLE','SEIS_SET';
go

execute sp_addextendedproperty 'Description','SEISMIC SET AREA: This table can be used to associate a seismic set with areas.','USER','dbo','TABLE','SEIS_SET_AREA';
go

execute sp_addextendedproperty 'Description','SEISMIC SET AUTHORIZATION: this table is used to track the level of authorizations given by owning organizations pertaining to the sale and disposotion of seismic data for a line.','USER','dbo','TABLE','SEIS_SET_AUTHORIZE';
go

execute sp_addextendedproperty 'Description','SEISMIC SET COMPONENT: This table is used to capture the relationships between seismic sets and busines objects, such as wells, equipment, documents and contracts.','USER','dbo','TABLE','SEIS_SET_COMPONENT';
go

execute sp_addextendedproperty 'Description','SEISMIC SET JUJRISDICTION: This table can be used to associate a seismic set with the jurisdications that control all or part of operations.','USER','dbo','TABLE','SEIS_SET_JURISDICTION';
go

execute sp_addextendedproperty 'Description','SEISMIC SET PLANNED: This type of seismic set has not been acquired as yet, and exists only in the planning stages. When actually acquired, create the appropriate type of seismic set based on acquisition. This table allows the planned set to be associated with a planned acquisition design, geometry, vessel and layout etc.','USER','dbo','TABLE','SEIS_SET_PLAN';
go

execute sp_addextendedproperty 'Description','SEISMIC SET STATUS: The status of any seismic set, qualified by the type of status. For example, the planning status of an acquisition set may be APPROVED, the financial status may be AUTHORIZED and the operational status may be COMPLETE.','USER','dbo','TABLE','SEIS_SET_STATUS';
go

execute sp_addextendedproperty 'Description','SEISMIC SHOT POINT SURVEY LOCATION: break-out table enabling a linkage between survey points and monuments.','USER','dbo','TABLE','SEIS_SP_SURVEY';
go

execute sp_addextendedproperty 'Description','SEISMIC STREAMER: this table is used to capture the description of a marine seismic streamer used during seismic acquisition. The streamer may be built of numerous components that may be described in SEIS STREAMER BUILD or SEIS STREAMER COMP.','USER','dbo','TABLE','SEIS_STREAMER';
go

execute sp_addextendedproperty 'Description','SEISMIC STREAMER BUILD: this table allows you to capture the type and position of each component of a streamer. If less detail is wanted, use SEIS STREAMER COMP to capture only the type and total count of each component on the streamer.','USER','dbo','TABLE','SEIS_STREAMER_BUILD';
go

execute sp_addextendedproperty 'Description','SEISMIC STREAMER COMPONENT: This table is used to capture the types of components on the streamer and the count of each type. If additional detail about the position of each component is needed, use SEIS STREAMER BUILD.','USER','dbo','TABLE','SEIS_STREAMER_COMP';
go

execute sp_addextendedproperty 'Description','SEISMIC TRANSACTION: a transaction between two business associates that results in the exchange of data. May be a sale, purchase, inspection etc.','USER','dbo','TABLE','SEIS_TRANSACTION';
go

execute sp_addextendedproperty 'Description','SEISMIC TRANSACTION COMPONENT: this table lists the business objects involved in a seismic transaction.','USER','dbo','TABLE','SEIS_TRANS_COMPONENT';
go

execute sp_addextendedproperty 'Description','SEISMIC VELOCITY: This table is used to capture seismic velocities that are created through seismic or well processing and interpretation.','USER','dbo','TABLE','SEIS_VELOCITY';
go

execute sp_addextendedproperty 'Description','SEISMIC VELOCITY INTERVAL: This table contains information pertaining to a method determining the average velocity as a function of depth. Velocities are obtained from the sonic log, calibration of the velocit ies from check shots, and densities froma density log.','USER','dbo','TABLE','SEIS_VELOCITY_INTERVAL';
go

execute sp_addextendedproperty 'Description','SEISMIC VELOCITY VOLUME: Captures details about a velocity volume. The volume itself may be captured in the table SEIS VELOCITY or it may be referenced as a stored digital file in the RM module.','USER','dbo','TABLE','SEIS_VELOCITY_VOLUME';
go

execute sp_addextendedproperty 'Description','SEISMIC VESSEL: This table captures the use of a vessel during acquisition of seismic data. Details about the vessel configuration that are specific to this acquisition are captured in this table or subordinates. Fixed details about the vessel and its ownership are captured in the support facility module.','USER','dbo','TABLE','SEIS_VESSEL';
go

execute sp_addextendedproperty 'Description','SEISMIC SET - WELL CHECKSHOT SURVEY: The Well Checkshot Survey table contains well check shot survey information. Check shots in a well are made by firing a seismic source near the surface, and detecting it with a geophone suspended at a series of depth s in the well. They are called check shots because they are shots to check a sonic log.','USER','dbo','TABLE','SEIS_WELL';
go

execute sp_addextendedproperty 'Description','SUPPORTING FACILITY - AIRCRAFT: This table is used to describe aircraft such as airplanes and helicopters that are used to support operations.','USER','dbo','TABLE','SF_AIRCRAFT';
go

execute sp_addextendedproperty 'Description','SUPPORTING FACILITY - AIRSTRIP: This table is used to capture information about airstrips used to support operations.','USER','dbo','TABLE','SF_AIRSTRIP';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY ALIAS: Support facilities may have more than one name, code or identifier. Variations can be stored here.','USER','dbo','TABLE','SF_ALIAS';
go

execute sp_addextendedproperty 'Description','AREA: This table can be used to associate a seismic set with areas.','USER','dbo','TABLE','SF_AREA';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY BA CREW: Use this table to track the relationships between support faciities and crews. Crews may be permanent or temporary, and details about the crew are tracked in BA CREW MEMBER.','USER','dbo','TABLE','SF_BA_CREW';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY SERVICE: this table may be used to track services that are provided for a facility, such as maintenance, inspections, supplies etc.','USER','dbo','TABLE','SF_BA_SERVICE';
go

execute sp_addextendedproperty 'Description','SUPPORTING FACILITY - BRIDGE: this table is used to capture information about bridges used to cross bodies of water, gorges or other obstacles. Any type of bridge may be described, from foot bridges to ice bridges that are only open in winter.','USER','dbo','TABLE','SF_BRIDGE';
go

execute sp_addextendedproperty 'Description','SUPPORTING FACILITY COMPONENT: this table is used to track relationships between support facilities of all types and other business objects. For example, you can track the relationship between a well and the road that services it, or the relationship between a well and the pad or platform on which it is constructed. Could also be used to track the roads used to service a well and the rate schedule applied to its use.','USER','dbo','TABLE','SF_COMPONENT';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY DESCRIPTION: This generic table may be used to track descriptive information about support facilities that is not contained in the subtype tables.','USER','dbo','TABLE','SF_DESCRIPTION';
go

execute sp_addextendedproperty 'Description','SUPPORTING FACILITY - DISPOSAL: A support facility or site that is used for disposal, usually during field operations or production operations.','USER','dbo','TABLE','SF_DISPOSAL';
go

execute sp_addextendedproperty 'Description','SUPPORTING FACILITY -DOCK: This table is used to capture information about docks used for support of operations.','USER','dbo','TABLE','SF_DOCK';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITIES - ELECTRIC FACILITIES: various types of electric related facilities can be described here, from power poles to transmission towers and plants.','USER','dbo','TABLE','SF_ELECTRIC';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY EQUIPMENT: Use this table to describe all the equipment that is included on the support facility such as a rig. In rigs, equipment may include engines, generators, mud pumps, drawworks, mast, substructure, traveling block, swivel, rotary table, blowout preventor, blowout preventor closing units etc.','USER','dbo','TABLE','SF_EQUIPMENT';
go

execute sp_addextendedproperty 'Description','SUPPORTING FACILITY - HABITAT: A support facility that is used as a habitat or working environment, such as a trailer, camp or office building.','USER','dbo','TABLE','SF_HABITAT';
go

execute sp_addextendedproperty 'Description','SUPPORTING FACILITY - LANDING: A support facility that is used for landing, such as a heliport. May be n airstrip or a helipad (A prepared area designated and used for takeoff and landing of helicopters. (Includes touchdown or hover point.) ) or a heliport (A facility designated for operating, basing, servicing, and maintaining helicopters).','USER','dbo','TABLE','SF_LANDING';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY MAINTENANCE: the maintenance record for the support facility, including who performed the maintenance, what was done and when it was done. Schedules may also be held here.','USER','dbo','TABLE','SF_MAINTAIN';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY MONUMENT: a position for which a location must be permanently tracked. Contains survey monuments, bench marks, GPS stations etc. used to position well locations, seismic points and other point of interest.','USER','dbo','TABLE','SF_MONUMENT';
go

execute sp_addextendedproperty 'Description','SUPPORTING FACILITY - OTHER: A generic support facility that has been created to allow additional subtypes to be managed. Addition of new subtypes should be submitted to PPDM for inclusion in the model.','USER','dbo','TABLE','SF_OTHER';
go

execute sp_addextendedproperty 'Description','SUPPORTING FACILITY - PAD: A pad is a supporting structure on which land wells are constructed. Many wells may be constructed on a single pad.','USER','dbo','TABLE','SF_PAD';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILTY - PLATFORM: The Platform table contains information pertaining to a fixed drilling location (either an offshore platform or an onshore pad). The offshore platform which is a flat working surface supported above the ocean surface and used for offshoredrilling. A drilling pad is a flat working surface on the ground used in onshore drilling for drilling multiple wellbores to reduce environmental impact.','USER','dbo','TABLE','SF_PLATFORM';
go

execute sp_addextendedproperty 'Description','SUPPORTING FACILITY - PORT: A support facility used to describe ports or harbors where vessels may dock to load or unload material.','USER','dbo','TABLE','SF_PORT';
go

execute sp_addextendedproperty 'Description','SUPPORTING FACILITY - RAILWAY: this table is used to describe railways used to support operations.','USER','dbo','TABLE','SF_RAILWAY';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY RESTRICTION: Surface restrictions placed on a support facility, such as operational activities limitations. For example, some roads may only be accessible in winter.','USER','dbo','TABLE','SF_RESTRICTION';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY RESTRICTION REMARK: A text description to provide additional information about a surface restriction which could impact on operations. Remarks may be used to clarify the times or seasons that the restriciton is active to to describe the administrative requirements for the restriction.','USER','dbo','TABLE','SF_REST_REMARK';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY RIG: The equipment that is used to drill a wellbore. In onshore operations, the rig is situated on a stable drilling pad, and includes the equipment used the drill the wellbore and the equipment necessary to support it, such as mud tanks, drawworks, masts and rotary table. Offshore rigs include the same main components as onshore rigs, but are situated on a vessel of drilling platform, and are configured to allow the drilling apparatus to pass through water before drilling into the earths surface.','USER','dbo','TABLE','SF_RIG';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY RIG BLOWOUT PREVENTER: A large valve at the top of a well that may be closed if the drilling crew loses control of formation fluids. By closing this valve (usually operated remotely via hydraulic actuators), the drilling crew usually regains controlof the reservoir, and procedures can then be initiated to increase the mud density until it is possible to open the BOP and retain pressure control of the formation. BOPs come in a variety of styles, sizes and pressure ratings. Some can effectively close over an open wellbore, some are designed to seal around tubular components in the well (drillpipe, casing or tubing) and others are fitted with hardened steel shearing surfaces that can actually cut through drillpipe. Since BOPs are critically important to the safety of the crew, the rig and the wellbore itself, BOPs are inspected, tested andrefurbished at regular intervals determined by a combination of risk assessment, local practice, well type and legal requirements. BOP tests vary from daily function testing on critical wells to monthly or less frequent testing on wells thought to have low probability of well control problems. (Schlumberger Oilfield Glossary).','USER','dbo','TABLE','SF_RIG_BOP';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY RIG GENERATOR: Use this table to describe the power generators and plants, such as lighting, power or compressor plants on the rig. Details such as manufacturer and detailed specifications should be captured in the EQUIPMENT module, but some key parameters are replicated here.','USER','dbo','TABLE','SF_RIG_GENERATOR';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY RIG OVERFHEAD EQUIPMENT: this table to describe the overhead equipment on the rig, such as the travelling block or the swivel. Details such as manufacturer and detailed specifications should be captured in the EQUIPMENT module, but some key parameters are replicated here.','USER','dbo','TABLE','SF_RIG_OVERHEAD_EQUIP';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY RIG PUMP: Use this table to describe the pumps on the rig. Details such as manufacturer and detailed specifications should be captured in the EQUIPMENT module, but some key parameters are replicated here.','USER','dbo','TABLE','SF_RIG_PUMP';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY RIG SHKER: The primary and probably most important device on the rig for removing drilled solids from the mud. This vibrating sieve is simple in concept, but a bit more complicated to use efficiently. A wire-cloth screen vibrates while the drilling fluid flows on top of it. The liquid phase of the mud and solids smaller than the wire mesh pass through the screen, while larger solids are retained on the screen and eventually fall off the back of the device and are discarded. Obviously, smaller openings in the screen clean more solids from the whole mud, but there is a corresponding decrease in flow rate per unit area of wire cloth. Hence, the drilling crew should seek to run the screens (as the wire cloth is called), as fine as possible, without dumping whole mud off the back of the shaker. Where it was once common for drilling rigs tohave only one or two shale shakers, modern high-efficiency rigs are often fitted with four or more shakers, thus giving more area of wire cloth to use, and giving the crew the flexibility to run increasingly fine screens. (Schlumberger Oilfield Glossary).','USER','dbo','TABLE','SF_RIG_SHAKER';
go

execute sp_addextendedproperty 'Description','SUPPORTING FACILITY ROAD: this table is used to describe roads used in support of operations. Each row in the table may represent an entire road or a portion of a road defined for some specific purpose (such as having its own fee structure or ownership or for variations in the composition of the road surface). Relationships between road segements are captured in the SF XREF table.','USER','dbo','TABLE','SF_ROAD';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY STATUS: the status of the support facility as it changes over time. Different types of statuses, such as construction, operational, financial statuses may be tracked.','USER','dbo','TABLE','SF_STATUS';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY: A support facility provides operational support for activities. These facilities may include roads, transmission towers, airstrips, vessels, docks and so on. Sub tyes are used to fully describe each type of support facility. All relationships with other modules are managed through the parent table.','USER','dbo','TABLE','SF_SUPPORT_FACILITY';
go

execute sp_addextendedproperty 'Description','SUPPORTING FACILITY - TOWER: this table is uses to capture information about towers used to support operations. These towers may be microwave towers, radio towers etc.','USER','dbo','TABLE','SF_TOWER';
go

execute sp_addextendedproperty 'Description','SUPPORTING FACILITY - VEHICLE: A land veicle may be used to support activities, transportation of materials (either hydrocarbons or supplies) or for seismic acquisition. This table may be used to describe various types of vehicles, such as trucks, cars, ATV etc.','USER','dbo','TABLE','SF_VEHICLE';
go

execute sp_addextendedproperty 'Description','SUPPORTING FACILITY - VESSEL: A marine vessel may be used to support production activities, transportation of materials (either hydrocarbons or supplies) or for seismic acquisition. This table is used to describe the basic configuration of each vessel.','USER','dbo','TABLE','SF_VESSEL';
go

execute sp_addextendedproperty 'Description','SUPPORTING FACILITY - WASTE DISPOSAL: May be a formal waste disposal facility that is mechanized or designed to handle toxic or hazardous materials, or a waste disposal pit that is dug to store certain types of non-dangerous waste.','USER','dbo','TABLE','SF_WASTE';
go

execute sp_addextendedproperty 'Description','SUPPORT FACILITY WASTE DISPOSAL: Use this table to track information about waste disposal from a well, facility or HSE Incident.','USER','dbo','TABLE','SF_WASTE_DISPOSAL';
go

execute sp_addextendedproperty 'Description','SUPPORTING FACILITY CROSS REFERENCE: this table is used to capture relationships between support facilities. You can use this table to track the associations between road segmennts, towers and roads, bridges and roads etc.','USER','dbo','TABLE','SF_XREF';
go

execute sp_addextendedproperty 'Description','SOURCE DOCUMENT: A list of documents that are used as the source for supplying data. Source documents may include government and regulatory forms, operator reports or scout tickets, publications, news letters, books or jounals.','USER','dbo','TABLE','SOURCE_DOCUMENT';
go

execute sp_addextendedproperty 'Description','SOURCE DOCUMENT AUTHOR: this table may be used to list the names of the authors of a source document.','USER','dbo','TABLE','SOURCE_DOC_AUTHOR';
go

execute sp_addextendedproperty 'Description','SOURCE DOCUMENT BIBLIOGRAPHY: This table can be used to capture the bibliographic references made in a source document. These references may be validated as other source documents or unvalidated with a simple document name text field.','USER','dbo','TABLE','SOURCE_DOC_BIBLIO';
go

execute sp_addextendedproperty 'Description','SPACING UNIT: a spatial description, described in one or more instruments, created by a jurisdictional body (such as a government) which imposes restrictions on well related activities, such as drilling or producing.','USER','dbo','TABLE','SPACING_UNIT';
go

execute sp_addextendedproperty 'Description','SPACING UNIT INSTRUMENT: may be used to track the instruments that are associated with a spacing unit.','USER','dbo','TABLE','SPACING_UNIT_INST';
go

execute sp_addextendedproperty 'Description','SPATIAL DESCRIPTION: The surface and sub-surface description of land. The surface description may be stated in terms of a legal survey system, metes and bounds or polygon. The mineral zone description describes the minerals (substances) and subsurface definition (zones/formations) to which rights are granted. For land rights, continuations may be described by generating a new LLD.','USER','dbo','TABLE','SPATIAL_DESCRIPTION';
go

execute sp_addextendedproperty 'Description','SPATIAL POLYGON BOUNDARY: This table contains the points which outline the perimeter of a polygon. The points are sequenced spatially in either a clockwise or counter clockwise direction, as specified in SP_POLYGON.','USER','dbo','TABLE','SP_BOUNDARY';
go

execute sp_addextendedproperty 'Description','SPATIAL DESCRIPTION POINT VERSION: Carries all the different coordinate values associated with a point which must be kept permanently. Point versions may be in alternate survey systems, use different map projections, use different survey methods or tools. Original locations may be stored in this table as desired. This point version table is designed for POLYGONAL geometries.','USER','dbo','TABLE','SP_BOUNDARY_VERSION';
go

execute sp_addextendedproperty 'Description','SPATIAL DESCRIPTION COMPONENT: This table is used to identify the business objects that are defined by this spatial description. The relationship supports the business rules that an object may have one or more spatial descriptions and that a spatial description may describe one or more business objects. It would be common for example, for a land right and a contract to be described using the same spatial description.','USER','dbo','TABLE','SP_COMPONENT';
go

execute sp_addextendedproperty 'Description','SPATIAL DESCRIPTION TEXT: Describes the spatial extent of the land right, both on the surface and at the mineral zone. Usually textual in nature, this information may be very lengthy and detailed. GIS or SQL searches and functions cannot be readily performedon this information, but it is needed for reporting and field purposes.','USER','dbo','TABLE','SP_DESC_TEXT';
go

execute sp_addextendedproperty 'Description','SPATIAL DESCRIPTION CROSS REFERENCE: Allows relationships between the spatial descriptors for various PPDM items to be described. Could use to indicate spatial relationship between LAND RIGHTS and FIELDS or SPACING UNITS, for example. Can also be used to associate versions of the spatial descriptions with reasons why the spatial description changed. In the case of land rights, the spatial description may change as a result of continuations.','USER','dbo','TABLE','SP_DESC_XREF';
go

execute sp_addextendedproperty 'Description','SPATIAL LINE: a line that describes the linear extent of a spatial object such as a road, pipeline or transmission line.','USER','dbo','TABLE','SP_LINE';
go

execute sp_addextendedproperty 'Description','SPATIAL LINE POINT BOUNDARY: This table contains the points along a line.','USER','dbo','TABLE','SP_LINE_POINT';
go

execute sp_addextendedproperty 'Description','SPATIAL DESCRIPTION POINT VERSION: Carries all the different coordinate values associated with a point which must be kept permanently. Point versions may be in alternate survey systems, use different map projections, use different survey methods or tools. Original locations may be stored in this table as desired. This point version table is designed for LINEAR geometries.','USER','dbo','TABLE','SP_LINE_POINT_VERSION';
go

execute sp_addextendedproperty 'Description','SPATIAL MINERAL ZONE: Definition of the subsurface that is included in the spatial description. For example, subsurface rights that have been granted to the holder of the mineral agreement. These rights include an interval defined by either a formation top/base or depth top/base as well as substance rights. In some cases, the zone has been defined in terms of a particular well using a specific method (dual induction log or stratigraphic analysis). Standardized ones that are created by a regulatory or licencing agency can be defined using LAND ZONE DEFINITION.','USER','dbo','TABLE','SP_MINERAL_ZONE';
go

execute sp_addextendedproperty 'Description','SPATIAL PARCEL: This table is used to describe which land parcels are covered by a spatial descsription. Land parcels themselves are selected from specific lists of parcel descriptions in the tables SP_PARCEL_XXX.','USER','dbo','TABLE','SP_PARCEL';
go

execute sp_addextendedproperty 'Description','SPATIAL AREA: Use this table to relate land parcel areas in the SP_% tables to AREA table.','USER','dbo','TABLE','SP_PARCEL_AREA';
go

execute sp_addextendedproperty 'Description','SPATIAL PARCEL CARTER: this table provides an ennumerated list of the parcels included in the carter survey system. These parcels may be referenced when describing the spatial extent of a business object such as a land right, field or pool.','USER','dbo','TABLE','SP_PARCEL_CARTER';
go

execute sp_addextendedproperty 'Description','SPATIAL PARCEL CONGRESSIONAL: an ennumerated list of the parcels included in the congressional survey system. These parcels may be referenced when describing the spatial extent of a business object such as a land right, field or pool.','USER','dbo','TABLE','SP_PARCEL_CONGRESS';
go

execute sp_addextendedproperty 'Description','SPATIAL PARCEL DOMINION LAND SURVEY: an ennumerated list of the parcels included in the DLS survey system. These parcels may be referenced when describing the spatial extent of a business object such as a land right, field or pool.','USER','dbo','TABLE','SP_PARCEL_DLS';
go

execute sp_addextendedproperty 'Description','SPATIAL PARCEL DOMINION LAND SURVEY ROAD ALLLOWANCES: an ennumerated list of the parcels included in the DLS Road allowances. These parcels may be referenced when describing the spatial extent of a business object such as a land right, field or pool.','USER','dbo','TABLE','SP_PARCEL_DLS_ROAD';
go

execute sp_addextendedproperty 'Description','SPATIAL PARCEL FEDERAL PERMIT SYSTEM: an ennumerated list of the parcels included in the FPS Survey system. These parcels may be referenced when describing the spatial extent of a business object such as a land right, field or pool.','USER','dbo','TABLE','SP_PARCEL_FPS';
go

execute sp_addextendedproperty 'Description','SPATIAL PARCEL LIBYA: this table provides an ennumerated list of the parcels included in the Libyan survey system. These parcels may be referenced when describing the spatial extent of a business object such as a land right, field or pool.','USER','dbo','TABLE','SP_PARCEL_LIBYA';
go

execute sp_addextendedproperty 'Description','SPATIAL PARCEL LOT: The irregular divisions of land found in certain survey systems, such as Congressional. These parcels may be fully described in this table, and polygonal outlines defined using SPATIAL POLYGON.','USER','dbo','TABLE','SP_PARCEL_LOT';
go

execute sp_addextendedproperty 'Description','SPATIAL PARCEL MB: This table is used to describe the metes and bounds summary of locations within a land parcel. Land parcels themselves are selected from specific lists of parcel descriptions in the tables SP_PARCEL_XXX.','USER','dbo','TABLE','SP_PARCEL_M_B';
go

execute sp_addextendedproperty 'Description','SPATIAL PARCEL NE LOCATION: an ennumerated list of the parcels included in the NE LOCATION Survey system. These parcels may be referenced when describing the spatial extent of a business object such as a land right, field or pool.','USER','dbo','TABLE','SP_PARCEL_NE_LOC';
go

execute sp_addextendedproperty 'Description','SPATIAL PARCEL NORTH SEA LOCATION: an ennumerated list of the parcels included in the NORTH SEA LOCATION Survey system. These parcels may be referenced when describing the spatial extent of a business object such as a land right, field or pool.','USER','dbo','TABLE','SP_PARCEL_NORTH_SEA';
go

execute sp_addextendedproperty 'Description','SPATIAL PARCEL NATIONAL TOPOGRAPHIC SERIES: an ennumerated list of the parcels included in the NTS Survey system. These parcels may be referenced when describing the spatial extent of a business object such as a land right, field or pool.','USER','dbo','TABLE','SP_PARCEL_NTS';
go

execute sp_addextendedproperty 'Description','SPATIAL PARCELOFFSHORE LOCATION: an ennumerated list of the parcels included in the Offshore Survey system. These parcels may be referenced when describing the spatial extent of a business object such as a land right, field or pool.','USER','dbo','TABLE','SP_PARCEL_OFFSHORE';
go

execute sp_addextendedproperty 'Description','SPATIAL PARCEL OHIO LOCATION: an ennumerated list of the parcels included in the Ohio Survey system. These parcels may be referenced when describing the spatial extent of a business object such as a land right, field or pool.','USER','dbo','TABLE','SP_PARCEL_OHIO';
go

execute sp_addextendedproperty 'Description','SPATIAL PARCEL PUBLIC LAND BLOCK LOCATION: an ennumerated list of the parcels included in the Public land block Survey system. These parcels may be referenced when describing the spatial extent of a business object such as a land right, field or pool.','USER','dbo','TABLE','SP_PARCEL_PBL';
go

execute sp_addextendedproperty 'Description','SPATIAL PARCEL REMARK: remarks about the land parcel may be stored here. Should not be used to include information contained in other parts of the model.','USER','dbo','TABLE','SP_PARCEL_REMARK';
go

execute sp_addextendedproperty 'Description','SPATIAL PARCEL TEXAS LOCATION: an ennumerated list of the parcels included in the Texas Survey system. These parcels may be referenced when describing the spatial extent of a business object such as a land right, field or pool.','USER','dbo','TABLE','SP_PARCEL_TEXAS';
go

execute sp_addextendedproperty 'Description','SPATIAL POINT: a position for which a location must be permanently tracked. WELL NODE: Definition of a well point projected to the Earth"s surface with its "selected" coordinate value. SURFACE WELL NODE: a surface well position which must be kept permanently. SUBSURFACE WELL NODE: A subsurface wll location, such as a KOP which must be kept permanently.','USER','dbo','TABLE','SP_POINT';
go

execute sp_addextendedproperty 'Description','SPATIAL DESCRIPTION POINT VERSION: Carries all the different coordinate values associated with a point which must be kept permanently. Point versions may be in alternate survey systems, use different map projections, use different survey methods or tools. Original locations may be stored in this table as desired. This point version table is designed for POINT geometries.','USER','dbo','TABLE','SP_POINT_VERSION';
go

execute sp_addextendedproperty 'Description','SPATIAL POLYGON: a polygon which describes the outline of an area. Polygons may describe outlines of fields, pools, AOI agreements, land titles, land parcel lots, surface restrictions and others.','USER','dbo','TABLE','SP_POLYGON';
go

execute sp_addextendedproperty 'Description','SPATIAL ZONE DEFINITION: describes the well, method and product (type of log) used to define a formation (strat unit) to which rights have been granted. Usually a standardized zone that is defined by a regulatory body or agency.','USER','dbo','TABLE','SP_ZONE_DEFINITION';
go

execute sp_addextendedproperty 'Description','SPATIAL ZONE DEFINITION CROSS REFERENCE: this table is used in the relatively uncommon case in which a zone definition is replaced by a new zone definition. Usually, this is done by a regulatory agency. There is a business need to keep track of the historyof zone definitions used to describe land rights. When a new zone definition is created, the new and old definitions are related to each other in this table. Old zone definitions should not be deleted, they should be set to inactive.','USER','dbo','TABLE','SP_ZONE_DEFIN_XREF';
go

execute sp_addextendedproperty 'Description','SPATIAL ZONE SUBSTANCE: describes substances (and their related zone) which are specifically included or excluded from the spatial description. For example, land rights may be granted from surface to basement, except for gas in Zone A.','USER','dbo','TABLE','SP_ZONE_SUBSTANCE';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC ACQUISITION METHOD: This table is used when it is desirable to indicate how, in general terms, the Stratigraphic analysis was arrived at. If more detailed information is required, the table STRAT ACQTN COMPONENT may be used to capture explicit foreign keys for database objects that were used.','USER','dbo','TABLE','STRAT_ACQTN_METHOD';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC ALIAS: An alternate or alias name or identifier for a stratigraphic unit. In PPDM, both the preferred version and the alias version exist as strat units, the alias relationship between them is described in this table. Aliases are provided to allow conversion of names to preferred names during queries or data loads.','USER','dbo','TABLE','STRAT_ALIAS';
go

execute sp_addextendedproperty 'Description','STRAT COLUMN: an ordered sequence of STRAT UNITS which are representative of the conceptual interpretation of local or regional stratigraphy. It may represent biostratigraphic, lithostratigraphic, sequence stratigraphic, type section or a proposed bore hole. The geographic area of the interpreation is defined. A single STRAT COLUMN may be composed of STRAT UNITS that are derived from different STRAT NAME SETS.','USER','dbo','TABLE','STRAT_COLUMN';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC COLUMN ACQUISITION: this table may be used to list wells, cores, logs, seismic lines, projects, other interpretations etc. that were used for the acquisition of the stratigraphic interpretation of a field station, well section or stratigraphic column.','USER','dbo','TABLE','STRAT_COLUMN_ACQTN';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC COLUMN UNIT: This table is used to indicate what stratigraphic units have been interpreted to exist in the stratigraphic column including the order in which they are expected to occur and depths at which they may be expected to be found.','USER','dbo','TABLE','STRAT_COLUMN_UNIT';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC COLUMN CROSS REFERENCE:: This table may be used to capture the relationships between stratigraphic columns. For example, the Midwest Plains stratigraphic column may be a more generalized representation of several columns describing smaller areas.','USER','dbo','TABLE','STRAT_COLUMN_XREF';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC COLUMN UNIT AGE: the interpreted age of a stratigraphic unit within the context of an intepreted stratigraphic column.','USER','dbo','TABLE','STRAT_COL_UNIT_AGE';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC EQUIVALENCE: this table is used to capture the relationship defined between two units, two surfaces, or a unit and surface based on an interpretation that the two strat elements are the same age (equivalent stratigraphically and/or geochronologically), although they are seperated in space.','USER','dbo','TABLE','STRAT_EQUIVALENCE';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC ACQUISITION WELL CROSS REFERENCE: this table may be used to list wells that were used for the acquisition of the stratigraphic interpretation of a field station, well section or stratigraphic column.','USER','dbo','TABLE','STRAT_FIELD_ACQTN';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC FIELD NODE: a location in a stratigraphic field station that is retained for long term reference. These locations may be defined horizontally across the surface of the field station, or vertically down a test hole.','USER','dbo','TABLE','STRAT_FIELD_NODE';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC FIELD SECTION: Interpretation data for a Stratigraphic field station. Stratigraphic units that are interpreted to be part of the field section may be listed in addition to the order of occurance, measured distance perpendicular to direction of strike, indication of missing formations etc.','USER','dbo','TABLE','STRAT_FIELD_SECTION';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC FIELD STATION: any location where geological studies or analysis or observations are carried out, such as at a measured section, outcrop etc. LITHOLOGIC MEASURED SECTION: an aggegate description record of the stratigraphic thickness and lithology.','USER','dbo','TABLE','STRAT_FIELD_STATION';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC FIELD STATION INTERPRETED AGE: The interpreted age of a stratigraphic unit interpreted to be in a field station. Ages may be expressed in ordinal or chronologic terms.','USER','dbo','TABLE','STRAT_FLD_INTERP_AGE';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC HIERARCHY: The hierarchical relationships defining the categorization and classification of rock units and the various sub-divisions of the various catrgories. For example, the categorization of a STRAT UNIT as a "GROUP" places it in a higher category than a "FORMATION".','USER','dbo','TABLE','STRAT_HIERARCHY';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC HIERARCHY DESCRIPTION: This table may be used to capture the general structure of a stratigraphic hierarchy. For example, a chronostratigraphic hierarchy may be structured as Era, Period, Series, Stages. A logical parent table that capturesthe set of hierarchies is not defined in PPDM but is denormalized into this table.','USER','dbo','TABLE','STRAT_HIERARCHY_DESC';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC INTERPRETATION CORRELATION: An interpretation that establishes correlations between well sections and / or field sections based on some critieria, such as age or lithology. It is necessary that the interpretations that the correlations are based on were made using the same methodology, such as chronostratigraphy, sequence stratigraphy, biostratigraphy or lithostratigraphy.','USER','dbo','TABLE','STRAT_INTERP_CORR';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC NAME SET: A stratigraphic name set is an unordered collection of stratigraphic units, that may be in use for a Lexicon, a geographic area, a project, a company etc.','USER','dbo','TABLE','STRAT_NAME_SET';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC NAME SET CROSS REFERENCE: The relationship between stratigraphic name sets. For example, Name set A may be a subset of B, may replace C or may be the source from which D was derived.','USER','dbo','TABLE','STRAT_NAME_SET_XREF';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC NODE VERSION: alternate location versions for a stratigraphic field station node, based on other survey systems.','USER','dbo','TABLE','STRAT_NODE_VERSION';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC TOPOLOGICAL RELATIONSHIP: Describes the physical relationship between two stratigraphic units, including bounding relationships, over / underlying relationships, adjacency etc.','USER','dbo','TABLE','STRAT_TOPO_RELATION';
go

execute sp_addextendedproperty 'Description','STRAT UNIT: a body of rock, of significant and distinctive occurrence, distinguished from adjacent rock bodies on the basis of any one or more primary properties or attributes that rocks possess, such as mineral or fossil assemblages lithologic characteristics, environment and other natural occurrences.','USER','dbo','TABLE','STRAT_UNIT';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC UNIT AGE: the age of a stratigraphic unit outside the context of a physical occurance in a well or field station. Ages may be described in ordinal or chronological terms.','USER','dbo','TABLE','STRAT_UNIT_AGE';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHY UNIT COMPONENT: This table is used to capture the relationships between stratigraphy units and busines objects, such as wells, equipment, documents, seismic sets and contracts.','USER','dbo','TABLE','STRAT_UNIT_COMPONENT';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC UNIT DESCRIPTION: description of the characteristics of a stratigraphic unit, generally expressed in codified terms. In many cases, these descriptions are derived from Lexicons.','USER','dbo','TABLE','STRAT_UNIT_DESCRIPTION';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC WELL ACQUISITION: this table may be used to list wells, cores, logs, seismic lines, projects, other interpretations etc. that were used for the acquisition of the stratigraphic interpretation of a field station, well section or stratigraphic column.','USER','dbo','TABLE','STRAT_WELL_ACQTN';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC WELL INTERPRETATION AGE: the age of a stratigraphic unit as it occurs in the context of a well interpretation. Ages may be expressed in ordinal or chronological terms.','USER','dbo','TABLE','STRAT_WELL_INTERP_AGE';
go

execute sp_addextendedproperty 'Description','STRATIGRAPHIC WELL SECTION: Formerly called WELL FORMATION. The Well Section table contains information on well tops. This includes pick (positional) data on formations, markers, contacts and horizons that can be correlated from well to well within a geographic area.','USER','dbo','TABLE','STRAT_WELL_SECTION';
go

execute sp_addextendedproperty 'Description','SUBSTANCE: This table is used to store specific characteristics about the substance in question. For example, the range for the API gravity, the atomic mass, the carbon count, etc. The composition of each substance should be described in this table, SUBSTANCE COMPOSITION and SUBSTANCE PROPERTY DETAIL. Use SUBSTANCE TABLE and SUBSTANCE COLUMN to define which substances may be referenced in which PPDM tables. It is very important that the contents of this table be carefully managed by subject matter experts, with close attention to the definitions for each term. Be advised that many applications and agencies may use the same term, but the definition and composition details may not be the same. Use the SUBSTANCE XREF table to capture relationships between substances, such as similar substances.','USER','dbo','TABLE','SUBSTANCE';
go

execute sp_addextendedproperty 'Description','SUBSTANCE ALIAS: All possible names, codes and other identifiers can be stored here. While you may store preferred names in the SUBSTANCE table, you should do this procedurally from SUBSTANCE ALIAS.','USER','dbo','TABLE','SUBSTANCE_ALIAS';
go

execute sp_addextendedproperty 'Description','SUBSTANCE BUSINESS ASSOCIATE: Use this table to keep track of all the business associates who are involved with this substance. You can track who the substance is for, who the technician was, what lab the substance came from, who the scientist was, the manufacturer, available quantities, location, price and so on. Track the role played by the business associate in BA_ROLE. If a BA plays more than one role, create a row for each role played by the BA.','USER','dbo','TABLE','SUBSTANCE_BA';
go

execute sp_addextendedproperty 'Description','SUBSTANCE BEHAVIOR: This table contains lists of substances that can be described either chemically or in terms of composition. The manner in which the substance behaves should be indicated in this table, ie. does it behave as an element, an ion, an isotope, an isomer, a mineral, a chemical, a solvent, a production substance , or a hydrocarbon or number of these together. The composition of each substance should be described in this table, SUBSTANCE COMPOSITION and SUBSTANCE PROPERTY DETAIL. Use SUBSTANCE TABLE and SUBSTANCE COLUMN to define which substances may be referenced in which PPDM tables. It is very important that the contents of this table be carefully managed by subject matter experts, with close attention to the definitions for each term. Be advised that many applications and agencies may use the same term, but the definition and composition details may not be the same. Use the SUBSTANCE XREF table to capture relationships between substances, such as similar substances.','USER','dbo','TABLE','SUBSTANCE_BEHAVIOR';
go

execute sp_addextendedproperty 'Description','SUBSTANCE COMPOSITION: This table is used to describe the substances into which another substance can be broken down into. Use this table to define granularities or hierarchies amongst substances. Information about the processes used to create sub-substances from others is not defined in these tables.','USER','dbo','TABLE','SUBSTANCE_COMPOSITION';
go

execute sp_addextendedproperty 'Description','SUBSTANCE PROPERTY DETAIL: This table may be used to capture detailed descriptive information about substances that is not covered by the explicit columns in SUBSTANCE. A general veritcal property table, these values are controlled by PPDM PROPERTY SET and the control column SUBSTANCE PROPERTY TYPE.','USER','dbo','TABLE','SUBSTANCE_PROPERTY_DETAIL';
go

execute sp_addextendedproperty 'Description','SUBSTANCE USE: This table is used to summarize where it is appropriate or allowed to use specific substance definitions. For example, a contract may specify a particular substance and definition, or a regulation may use a particular substance (as defined). Thistable should be used to support the population and use of STUFF TABLE and STUFF COLUMN, which defines which SUBSTANCE is referenced in a particular case. In one table, it can be difficult to create a single definition for a substance (such as oil) that is appropriate for all applications of that term. You can use this table to refine exactly which definition should be used in which situation.','USER','dbo','TABLE','SUBSTANCE_USE';
go

execute sp_addextendedproperty 'Description','SUBSTANCE CROSS REFERENCE: Use this table to create relationships between substances other than compositional relationships (which should be stored in SUBSTANCE COMPOSITION). For example, relationships between a preferred substance and other entries in the SUBSTANCE table that may be deprecated, or converted to the preferred substance for some functions, or substances that are roughly equivalent can be stored in this table.','USER','dbo','TABLE','SUBSTANCE_XREF';
go

execute sp_addextendedproperty 'Description','WELL: A table for general and header information about a well. A well is an actual or proposed hole in the ground, designed to exchange fluids between a subsurface reservoir and the surface (or another reservoir) or to enable the detection and measurement of rock properties. A wellbore is a cylindrical hole created by a drill. A well may consist of zero, one, or more wellbores. Their relationships are described by the table WELL_XREF. Information from other well tables (e.g. key dates and depths) may be included (denormalized) for convenience. The term "well" is used in the column name and comments to mean "well", "wellbore", either, or both (depending on the context). The column WELL_LEVEL_TYPE is used to describe which type exists in a row of data.','USER','dbo','TABLE','WELL';
go

execute sp_addextendedproperty 'Description','WELL ACTIVITY: Use this table to track all activities and events in a well or well bore, including daily operations, downtime, prodution, operational or milestone events. Time and depth of the activity may be tracked.','USER','dbo','TABLE','WELL_ACTIVITY';
go

execute sp_addextendedproperty 'Description','WELL ACTIVITY CAUSE: Use this table when you want to track the conditions or events that caused this activity. Often used to classify the causes of downtime events.','USER','dbo','TABLE','WELL_ACTIVITY_CAUSE';
go

execute sp_addextendedproperty 'Description','WELL ACTIVITY COMPONENT: Use this table to keep track of objects created during an activity (logs, cores, tests), equipment used in an activity, events that are associated with an activity, or costs associated wtih an activity.','USER','dbo','TABLE','WELL_ACTIVITY_COMPONENT';
go

execute sp_addextendedproperty 'Description','WELL ACTIVITY DURATION: Use this table when you need to track the duration of events that span more than a day. You can track the period in which the activity ocurred, and the type of downtime that resulted (such as cessation of production, constrained production or deferred production).','USER','dbo','TABLE','WELL_ACTIVITY_DURATION';
go

execute sp_addextendedproperty 'Description','WELL ACTIVITY SET: This table describes sets of activity codes that may be in use. More than one set of codes may be used during drilling operations. A set of activity codes may be defined by a professional body such as the CAODC, by regulatory agencies, byservice companies or operators. These activity codes may otherwise be called Events, Statuses, Conditions, Occurences etc.','USER','dbo','TABLE','WELL_ACTIVITY_SET';
go

execute sp_addextendedproperty 'Description','WELL DRILLING ACTIVITY: This table contains a list of valid activities that occur in drilling or other well operations. Each set of activities is owned by a WELL DRILL ACTIVITY SET. For example rigging up, drilling ahead, setting casing, waiting on cement, fishing, sidetracking, coring, logging,testing, plugging or completing. This table should also be used to track status types and other information.','USER','dbo','TABLE','WELL_ACTIVITY_TYPE';
go

execute sp_addextendedproperty 'Description','WELL ACTIVITY TYPE ALIAS: The Well Activity Alias table contains names and identifiers that a well activity may otherwise be known as.','USER','dbo','TABLE','WELL_ACTIVITY_TYPE_ALIAS';
go

execute sp_addextendedproperty 'Description','WELL ACTIVITY TYPE EQUIVALENCE: as different organizations or regulatory agencies may name or group activities, differently, this table gives you an opportunity to state an equivalence between activities that are defined by different groups in different WELL ACTIVITY SETS.','USER','dbo','TABLE','WELL_ACTIVITY_TYPE_EQUIV';
go

execute sp_addextendedproperty 'Description','AIR DRILLING : The Air Drilling Header table contains information pertaining to the contractor and compressor sources used to perform an air drilling operation. Air drilling is rotary drilling that uses compressed air instead of a circulat ing mud system.','USER','dbo','TABLE','WELL_AIR_DRILL';
go

execute sp_addextendedproperty 'Description','AIR DRILLING INTERVAL: The Air Drilling Interval table contains depth interval and air volume information where air drilling was utilized in a wellbore. Air drilling is rotary drilling that uses compressed air instead of a circulating mud system.','USER','dbo','TABLE','WELL_AIR_DRILL_INTERVAL';
go

execute sp_addextendedproperty 'Description','AIR DRILLING INTERVAL PERIOD: This table may be used to associate an air drilling interval with the reporting or shift period(s) in which the drilling occurred.','USER','dbo','TABLE','WELL_AIR_DRILL_PERIOD';
go

execute sp_addextendedproperty 'Description','WELL ALIAS: The Well Alias table contains names and identifiers that a well may otherwise be known as. This would include previous or alternate well identifiers assigned to the well by a regulatory agency and the reason for the alias.','USER','dbo','TABLE','WELL_ALIAS';
go

execute sp_addextendedproperty 'Description','WELL AREA: this table tracks the relationships between wells and all areas thay they intersect with. These areas may be formal geopolitical areas, business or regulatory areas, informal areas etc.','USER','dbo','TABLE','WELL_AREA';
go

execute sp_addextendedproperty 'Description','WELL BUSINESS ASSOCIATE SERVICE: use this table to keep track of services that are performed for the well, including production strings and completions, by business associates. These may be various types of technical services, usually those not explicitly managed in other existing parts of the model.','USER','dbo','TABLE','WELL_BA_SERVICE';
go

execute sp_addextendedproperty 'Description','WELL CEMENT: A table to describe the cementing operations performed on a well, usually to secure and support a casing string or to restrict fluid movement between formations.','USER','dbo','TABLE','WELL_CEMENT';
go

execute sp_addextendedproperty 'Description','WELL COMPLETION: The Well Completion table identifies the completion activity in the wellbore. The well component WELLBORE COMPLETION should be captured as a row in the WELL table, with WELL_LEVEL_TYPE = WELLBORE COMPLETION. Since a wellbore may have multiple completions, the completion observation number uniquely identifies each occurrence. This table also describes the perforation operations conducted on a wellbore during the completion process.','USER','dbo','TABLE','WELL_COMPLETION';
go

execute sp_addextendedproperty 'Description','WELL COMPONENT: This table is used to capture the relationships between wells and busines objects, such as equipment, documents, seismic sets and contracts. Note that special relationships are often captured in detail tables. It will be necessary to review your options before selecting an implementation method.','USER','dbo','TABLE','WELL_COMPONENT';
go

execute sp_addextendedproperty 'Description','WELL CORE: The Well Core table contains descriptive information concerning a coring operation. Data contained in this table identifies the core type, conditions, and subsurface location of the core.','USER','dbo','TABLE','WELL_CORE';
go

execute sp_addextendedproperty 'Description','Z_WELL CORE ANALYSIS (TO BE DEPRECATED): The Well Core Analysis table describes the type of sample analytical data collected. Information contained in this table contains information such as sample shape, sample length, sample diameter, and analyst performing   the analysis etc. THIS TABLE IS SCHEDULED TO BE DEPRECATED, DO NOT USE THIS TABLE.  REFER TO THE ANALYSIS SUBJECT AREA.','USER','dbo','TABLE','WELL_CORE_ANALYSIS';
go

execute sp_addextendedproperty 'Description','Z_WELL CORE ANALYSIS METHOD (TO BE DEPRECATED): The Well Core Analysis Method table contains information pertaining to the methods used during a core analysis. For example,SOLVENT stores a code representing the type of solvent (such as toluene) used for distil  lation extraction. DRYING records how the core was dried after the extraction process has been completed,such as a high temperature oven or low temperature controlled humidity.  THIS TABLE IS SCHEDULED FOR DEPRECATION, AND IS CARRIED FORWARD TO ALLOW CURRENT USERS TO TRANSFER THIS FUNTION TO THE ANALYSIS SUBJECT AREA.  DO NOT USE THIS TABLE.','USER','dbo','TABLE','WELL_CORE_ANAL_METHOD';
go

execute sp_addextendedproperty 'Description','Z_WELL CORE ANALYSIS REMARK (TO BE DEPRECATED): The Well Core Analysis Remark table contains additional narrative remarks pertaining to the type of techniques used for core analysis. USE OF THIS TABLE WILL BE DEPRECATED, PLEASE REFER TO THE ANALYSIS SUBJECT AREA.','USER','dbo','TABLE','WELL_CORE_ANAL_REMARK';
go

execute sp_addextendedproperty 'Description','WELL CORE DESCRIPTION: The Well Core Description table contains textual lithological description and hydrocarbon shows that may appear in a core by geologic examination or chemical analysis.','USER','dbo','TABLE','WELL_CORE_DESCRIPTION';
go

execute sp_addextendedproperty 'Description','WELL CORE REMARK: The Well Core Remark table contains narrative remarks pertaining to a core. Comments could include a narrative describing the conventional and/or sidewall coring operations.','USER','dbo','TABLE','WELL_CORE_REMARK';
go

execute sp_addextendedproperty 'Description','WELL CORE SAMPLE ANALYSIS: The Well Core Sample Analysis table contains information pertaining to the methods used for measuring porosity, permeability, saturation and grain density in a well core sample analysis.','USER','dbo','TABLE','WELL_CORE_SAMPLE_ANAL';
go

execute sp_addextendedproperty 'Description','WELL CORE SAMPLE DESCRIPTION: The Well Core Sample Description table contains details about specific descriptions of a sample taken from a core which has been analyzed. Information pertains to such data as a hydrocarbon show, show quality , or porosity type etc.','USER','dbo','TABLE','WELL_CORE_SAMPLE_DESC';
go

execute sp_addextendedproperty 'Description','Z WELL CORE SAMPLE REMARK (TO BE DEPRECATED): The Well Core Sample Remark table contains narrative comments about the sample taken from a core which has been analyzed. THIS TABLE WILL BE DEPRECATED. PLEASE USE ANL_REMARK.','USER','dbo','TABLE','WELL_CORE_SAMPLE_RMK';
go

execute sp_addextendedproperty 'Description','WELL CORE SHIFT: The Well Core Shift table contains information needed to correct core measured depths to adjusted wireline log depths. The depth adjustment may be made for the entire core segment or individual intervals within the gross s egment.','USER','dbo','TABLE','WELL_CORE_SHIFT';
go

execute sp_addextendedproperty 'Description','WELL CORE FORMATION: The Well Core Formation table uniquely identifies the formation or stratigraphic unit present in the core. This table also identifies specific formations when a core has overlapped more than one formation.','USER','dbo','TABLE','WELL_CORE_STRAT_UNIT';
go

execute sp_addextendedproperty 'Description','WELL DIRECTIONAL SURVEY: The Well Directional Survey table contains header information about directional surveys which have  been performed on a wellbore. This downhole survey charts the  degree of departure of the wellbore from vertical and the direction of departure. Since many directional surveys can be conducted on  a wellbore, the survey number is included as part of the primary key to uniquely identify the survey.','USER','dbo','TABLE','WELL_DIR_SRVY';
go

execute sp_addextendedproperty 'Description','WELL DIRECTIONAL SURVEY COMPOSITE: this table allows the user to define a composite directional survey by calling sections of other existing directional surveys into the construction of a new survey. This is useful ifa new wellbore is drilled, but onlythe new portion has a directional survey. In this case, create a composite survey that connects the new survey and the necessary part of the directional survey from the original wellbore together.','USER','dbo','TABLE','WELL_DIR_SRVY_COMPOSITE';
go

execute sp_addextendedproperty 'Description','WELL DIRECTIONAL SURVEY STATION: The Well Directional Survey table records the individual directional survey points along the wellbore during a downhole survey. The measurements at the survey points record the inclination from the vertical  axis that the wellbore trends and the clockwise departure of the survey point from the north reference used in the directional survey. This table allows for multiple survey points or stations.','USER','dbo','TABLE','WELL_DIR_SRVY_STATION';
go

execute sp_addextendedproperty 'Description','WELL DIRECTIONAL SURVEY STATION: The Well Directional Survey table records the individual directional survey points along the wellbore during a downhole survey. The measurements at the survey points record the inclination from the vertical axis that the wellbore trends and the clockwise departure of the survey point from the north reference used in the directional survey. This table allows for multiple survey points or stations.','USER','dbo','TABLE','WELL_DIR_SRVY_ST_VERSION';
go

execute sp_addextendedproperty 'Description','WELL DIRECTIONAL SURVEY VERSION:  use this table to describe alternate versions of the directional survey.','USER','dbo','TABLE','WELL_DIR_SRVY_VERSION';
go

execute sp_addextendedproperty 'Description','WELL DRILL PERIOD ADDITIVE INVENTORY: This table is used to capture the inventoried amounts of each kind of additive at a specified point in a well drill period (usually shift, tour or day).','USER','dbo','TABLE','WELL_DRILL_ADD_INV';
go

execute sp_addextendedproperty 'Description','WELL DRILLING ASSEMBLY: a table that identifies a well drilling assembly used on a rig for a part of a reporting period or tour, or for more than one period. Each new assembly is represented by a row in this table, with descriptions of the individual components in WELL DRILL ASSEMBLY COMP and an indication of which drill periods the assembly was in use in WELL DRILL ASSEMBLY PER.','USER','dbo','TABLE','WELL_DRILL_ASSEMBLY';
go

execute sp_addextendedproperty 'Description','WELL DRILLING ASSEMBLY COMPONENT: Use this table to keep track of the component parts of a well drillilng assembly, not including the bit (in WELL DRILL BIT).','USER','dbo','TABLE','WELL_DRILL_ASSEMBLY_COMP';
go

execute sp_addextendedproperty 'Description','WELL DRILLING ASSEMBLY: a table that identifies a well drilling assembly used on a rig for a part of a reporting period or tour, or for more than one period.','USER','dbo','TABLE','WELL_DRILL_ASSEMBLY_PER';
go

execute sp_addextendedproperty 'Description','WELL DRILL BIT CONDITION: The table is used to track the condition of the well drill bit as it is evaluated over time. This information is used to decide what kinds of bits to use and to determine preformance statistics on bit types, and to ensure that maintenance is conducted as efficiently as possible.','USER','dbo','TABLE','WELL_DRILL_BIT_CONDITION';
go

execute sp_addextendedproperty 'Description','WELL DRILLING BIT INTERVAL: The Drilling Bit table describes the physical characteristics of the drill bit and the operating conditions while using the bit during a drilling operation. Describes information about the bit as reported during a run, orinterval across which the bit was used. Many of these values are calculated from information in WELL DRILL BIT PERIOD, as values are typically reported on a period by period basis.','USER','dbo','TABLE','WELL_DRILL_BIT_INTERVAL';
go

execute sp_addextendedproperty 'Description','WELL DRILL BIT JET: The table is used to describe each jet on a drill bit. In some cases, more than one kind of jet may be used on the drill bit.','USER','dbo','TABLE','WELL_DRILL_BIT_JET';
go

execute sp_addextendedproperty 'Description','WELL DRILL BIT PERIOD: This table may be used to describe characteristics of the use of a drill bit during a specified reporting period. Metrics that are stored are used to help calculate statistics and for regulatory reporting. Some of these figures are used to calculate values in WELL DRILL BIT INTERVAL.','USER','dbo','TABLE','WELL_DRILL_BIT_PERIOD';
go

execute sp_addextendedproperty 'Description','WELL DRILL CHECK: The daily checks refer to required or recommended rig inspections that are to be conducted by both the Operator and Contractor representatives, or by the Contractors representative alone. This section is important both as a reminder to conduct these inspections, and as a record of the inspection, for compliance with government regulations and industry practice.','USER','dbo','TABLE','WELL_DRILL_CHECK';
go

execute sp_addextendedproperty 'Description','WELL DRILL CHECK SET: Use this table to define sets of drill checks that may be mandated by an operator company, regulatory agency or contract specifications. Each check set consists of one or more checks, listed in WELL DRILL CHECK TYPE. Details about the daily checks are stored in WELL DRILL CHECK.','USER','dbo','TABLE','WELL_DRILL_CHECK_SET';
go

execute sp_addextendedproperty 'Description','WELL DRILL CHECK TYPE: The type of check that is conducted, such as Daily Walk Around Inspection, H2S Signs Posted (if required), Well License and Stick Diagram Posted, Flare Lines Staked, BOP Drills Performed, Visually inspect BOPs, Flarelines and Degasser Lines','USER','dbo','TABLE','WELL_DRILL_CHECK_TYPE';
go

execute sp_addextendedproperty 'Description','WELL DRILL PERIOD EQUIPMENT: This table is used to track how and where specific pieces of equipment are used, for lifetime use and history purposes. You can track any equipment and associate inventory items to this table to assist in easy query and retrieval.','USER','dbo','TABLE','WELL_DRILL_EQUIPMENT';
go

execute sp_addextendedproperty 'Description','WELL DRILL BIT INTERVAL DETAIL: use this table to capture additional details about the interval in which a drill bit was used. Any kind of detail is supportable, such as metrics.','USER','dbo','TABLE','WELL_DRILL_INT_DETAIL';
go

execute sp_addextendedproperty 'Description','WELL DRILL PERIOD MUD ADDITIVE: This table is used to capture a list of all the additives in the well mud during a reporting period or tour of operations. For underbalanced drilling, nitrificaiton media may be used, but this additive is non recoverable, and is typically managed a little differently in the field.','USER','dbo','TABLE','WELL_DRILL_MUD_ADDITIVE';
go

execute sp_addextendedproperty 'Description','DRILLING MUD / MEDIA INTERVAL: The Drilling Media Interval table contains information on the type of drilling media utilized during rotary drilling. Drilling media is used to cool and lubricate the bit, control subsurface fluids or build a filt er cake along the well walls.','USER','dbo','TABLE','WELL_DRILL_MUD_INTRVL';
go

execute sp_addextendedproperty 'Description','DRILLING MUD / MEDIA WEIGHT: The Drilling Mud / Media Weight table contains information pertaining to the density of drilling media, which is most commonly mud weight. It is measured in the field with a mud balance. Fresh water weight is 8.3 ppg, normal drilling mud weight is 9-10 ppg, and heavy drilling mud is 15-20 ppg. The heavier the mud weight, the greater the pressure it exerts on the bottom of the well.','USER','dbo','TABLE','WELL_DRILL_MUD_WEIGHT';
go

execute sp_addextendedproperty 'Description','WELL DRILLING PERIOD: Any period that is used to report well drilling operations, usually based on regulatory reporting requirements. Internationally, there may be 1, 2 or 3 periods each 24 hours. In some jurisdictions, reporting may occur for more than one period, such as for an 8 hour tour shift and a 24 daily summary. Periods may be as short or as long as needed.','USER','dbo','TABLE','WELL_DRILL_PERIOD';
go

execute sp_addextendedproperty 'Description','WELL DRILL PERIOD CREW: Use this table to track information about the crew or members of a crew at any given point in time. A crew member may be an individual or a company. The history of crew members may be tracked in this table, with currently activemembers indicated. A row of data may be for the entire crew or for an individual member of the crew.','USER','dbo','TABLE','WELL_DRILL_PERIOD_CREW';
go

execute sp_addextendedproperty 'Description','WELL DRILL PERIOD EQUIPMENT: This table is used to track how and where specific pieces of equipment are used, for lifetime use and history purposes. You can track any equipment and associate inventory items to this table to assist in easy query and retrieval.','USER','dbo','TABLE','WELL_DRILL_PERIOD_EQUIP';
go

execute sp_addextendedproperty 'Description','WELL DRILL PERIOD INVENTORY: Reports the amounts of materials, usually consumables, that are inventoried on site but not in the well bore at inventory time. Use this table to track amounts on hand, amounts ordered etc.','USER','dbo','TABLE','WELL_DRILL_PERIOD_INV';
go

execute sp_addextendedproperty 'Description','WELL DRILL PERIOD SERVICE: This table associates services described in WELL BA SERVICE to the drilling periods in which the service is provided. Metrics related to the service may be stored in this table as needed and defined.','USER','dbo','TABLE','WELL_DRILL_PERIOD_SERV';
go

execute sp_addextendedproperty 'Description','WELL DRILL PERIOD VESSEL: Describes information about the deployment or configuration of vessels during a reporting period.','USER','dbo','TABLE','WELL_DRILL_PERIOD_VESSEL';
go

execute sp_addextendedproperty 'Description','WELL DRILL PIPE INVENTORY: This table is used to track the inventory of pipe and tubular that is not in the well bore. Inventories are taken on a periodic reporting basis, such as shift or tour or day. Lists of pipe and tubulars that are installed in the well bore are tracked in WELL TUBULAR. Each row in this table should describe a single item or a group of items with the same characteristics.','USER','dbo','TABLE','WELL_DRILL_PIPE_INV';
go

execute sp_addextendedproperty 'Description','WELL DRILL PERIOD REMARK: The Well Drill Period Remark table contains additional information pertaining to well operations that are not covered by the detail tables.','USER','dbo','TABLE','WELL_DRILL_REMARK';
go

execute sp_addextendedproperty 'Description','DRILLING REPORT: Well drilling reports are often prepared and provided as summaries that cover one or more tours or shifts. All the tours (shifts) contained in a report must be for the same well. Reports for different wells should be captured as different reports. Calculate which shifts or tours are covered by a report by comparing the date and time ranges in WELL DRILL PERIOD and WELL DRILL REPORT.','USER','dbo','TABLE','WELL_DRILL_REPORT';
go

execute sp_addextendedproperty 'Description','WELL DRILL SHAKER: The primary and probably most important device on the rig for removing drilled solids from the mud. This vibrating sieve is simple in concept, but a bit more complicated to use efficiently. A wire-cloth screen vibrates while the drilling fluid flows on top of it. The liquid phase of the mud and solids smaller than the wire mesh pass through the screen, while larger solids are retained on the screen and eventually fall off the back of the device and are discarded. Obviously, smaller openings in the screen clean more solids from the whole mud, but there is a corresponding decrease in flow rate per unit area of wire cloth. Hence, the drilling crew should seek to run the screens (as the wire cloth is called), as fine as possible, without dumping whole mud off the back of the shaker. Where it was once common for drilling rigs to haveonly one or two shale shakers, modern high-efficiency rigs are often fitted with four or more shakers, thus giving more area of wire cloth to use, and giving the crew the flexibility to run increasingly fine screens. (Schlumberger Oilfield Glossary).','USER','dbo','TABLE','WELL_DRILL_SHAKER';
go

execute sp_addextendedproperty 'Description','WELL DRILLING AND OPERATIONS STATISTICS: Use this table to capture statistics or metrics for well operation periods that are not defined elsewhere. A WELL DRILL PERIOD can be for any length of time, including shift lengths, days, weeks, months, years etc.','USER','dbo','TABLE','WELL_DRILL_STATISTIC';
go

execute sp_addextendedproperty 'Description','WELL DRILL WEATHER: Use this table to capture weather or oceanic conditions during a well drilling period. As many readings of various types may be captured as you wish.','USER','dbo','TABLE','WELL_DRILL_WEATHER';
go

execute sp_addextendedproperty 'Description','WELL EQUIPMENT: ennumerates the equipment installed at the well.','USER','dbo','TABLE','WELL_EQUIPMENT';
go

execute sp_addextendedproperty 'Description','WELL FACILITY: This table provides associations between wells and facilities.','USER','dbo','TABLE','WELL_FACILITY';
go

execute sp_addextendedproperty 'Description','WELL HORIZONTAL DRILLING HEADER: The Horizontal Drilling Header table contains information about a horizontally drilled well, which is a new well drilled from a point in a well bore with lengths varying from 100 to 3,000 feet or more. This table stores the typeof horizontal well (Ultra Short, Short, Medium and Long radius).','USER','dbo','TABLE','WELL_HORIZ_DRILL';
go

execute sp_addextendedproperty 'Description','HORIZONTAL DRILLING KICKOFF POINT: The Horizontal Drilling Kickoff Point table contains information related to kickoff points. A kickoff point is the point where the well bore begins to deviate from vertical to horizontal or that point where a lateral becomes a spoke.','USER','dbo','TABLE','WELL_HORIZ_DRILL_KOP';
go

execute sp_addextendedproperty 'Description','HORIZONTAL DRILLING POINT OF ENTRY: The Horizontal Drilling Point Of Entry table contains information related to point of entry. The point of entry is that point where the horizontal wellbore penetrates the targeted formation/reservoir.','USER','dbo','TABLE','WELL_HORIZ_DRILL_POE';
go

execute sp_addextendedproperty 'Description','HORIZONTAL DRILLING SPOKE: The Horizontal Drilling Spoke table contains information related to a spoke terminus. A spoke terminus is the farthest extent of a spoke (the end point of each spoke).','USER','dbo','TABLE','WELL_HORIZ_DRILL_SPOKE';
go

execute sp_addextendedproperty 'Description','WELL LICENSE: The Well License table contains well license information identifying the licensee and drilling contractor authorized to explore and drill for petroleum on the tract of land covered by the license during the term of the licens e. Location and formation information are also stored in this table. In addition, the WELL_LICENSE table contains data pertaining to the drilling rig and other drilling tools used.','USER','dbo','TABLE','WELL_LICENSE';
go

execute sp_addextendedproperty 'Description','WELL LICENSE NAME ALIAS: The Alias table stores multiple alias names from applications, other owners etc.','USER','dbo','TABLE','WELL_LICENSE_ALIAS';
go

execute sp_addextendedproperty 'Description','WELL LICENSE AREA: This table provides a list of the areas that a well license falls into.','USER','dbo','TABLE','WELL_LICENSE_AREA';
go

execute sp_addextendedproperty 'Description','WELL LICENSE CONDITION: lists the conditions under which the license or approval has been granted. May include payment of fees, development of agreements, performance of work etc. If desired, the project module may be used to track fulfillment of operationalconditions. The obligations module is used to track payment of fees.','USER','dbo','TABLE','WELL_LICENSE_COND';
go

execute sp_addextendedproperty 'Description','WELL LICENSE REMARK: a text description to record general comments on the license tracking when remark was made, who is the author and the type of remark.','USER','dbo','TABLE','WELL_LICENSE_REMARK';
go

execute sp_addextendedproperty 'Description','WELL LICENSE STATUS: Tracks the status of a well license throughout its lifetime. Various types of status may be included at the discretion of the implementor.','USER','dbo','TABLE','WELL_LICENSE_STATUS';
go

execute sp_addextendedproperty 'Description','WELL LICENSE VIOLATION: Use this table to track incidents where the terms of a license have been violated (or perhaps are claimed to be violated). At this time the table is relatively simple in content.','USER','dbo','TABLE','WELL_LICENSE_VIOLATION';
go

execute sp_addextendedproperty 'Description','WELL LOG: A group of one or more curves. These curves, when taken together, are often assigned a name, such as Induction/Sonic, or FDC/CNL. When dealing with digitally delivered well log data, a log is generally synonymous with Pass or File. A File is the basic unit of digital well log data interchange. DLIS, LIS, and BIT are multi-file tape formats which can be encapsulated and created on, or copied to disk as a single physical file. Each logical file within this physical disk file is roughly comparable to the information contained in one LAS file.','USER','dbo','TABLE','WELL_LOG';
go

execute sp_addextendedproperty 'Description','WELL LOG AXIS COORDINATES: this table may be used to describe the coordinates on a well log curve axis.','USER','dbo','TABLE','WELL_LOG_AXIS_COORD';
go

execute sp_addextendedproperty 'Description','WELL LOG CLASS: This table lists the well log classes that a specific well log can be categorized as, such as CNL-FDC, CNL, FDC etc. .','USER','dbo','TABLE','WELL_LOG_CLASS';
go

execute sp_addextendedproperty 'Description','WELL LOG CLASS CURVE CLASS: this table is used to ennumerate the types of curve that are typically found in a specific class of well log.','USER','dbo','TABLE','WELL_LOG_CLS_CRV_CLS';
go

execute sp_addextendedproperty 'Description','WELL LOG CURVE CLASS CROSS REFERENCE: this table is intended to capture the hierarchical relationships between curve classes. For example, the parent curve class CONDUCTIVITY may include a variety of specific conductivity types, such as Array_Conductivity, Profile_Conductivity, Shallow_Conductivity, Water_Conductivity, Deep_Conductivity, Fluid_Conductivity, Flushed_Zone_Conductivity, Formation_Conductivity, Fracture_Conductivity, Medium_Conductivity, Micro_Conductivity and so on. Of these, some types may have their own sub types. ARRAY CONDUCTIVITYhas two children, Induction_Array_Conductivity and Laterolog_Array_Conductivity. Note that other types of relationships may also be stored, provided the R CURVE XREF TYPE table is properly populated.','USER','dbo','TABLE','WELL_LOG_CRV_CLS_XREF';
go

execute sp_addextendedproperty 'Description','WELL LOG CURVE: The Well Log Curve table contains information about the types of log curves or traces associated with a logging pass. Each pass of a logging tool string may produce many log curves since there are many logging tools on the tool string. Also know as a Channel, a Curve is a set of values with a corresponding index (e.g. depth or time) for each value. In digital well log interchange formats, a Curve may be associated with only one Frame. The simplest of curves contains one value for each index value of depth or time. Curves can, however, be very complexentities, containing multi-dimensional arrays of data values in each Frame.','USER','dbo','TABLE','WELL_LOG_CURVE';
go

execute sp_addextendedproperty 'Description','WELL LOLG CURVE AXIS: this table describes each axis of the curve in the case where multidimensional recording has occurred.','USER','dbo','TABLE','WELL_LOG_CURVE_AXIS';
go

execute sp_addextendedproperty 'Description','WELL LOG CURVE CLASS IDENTIFIER: A valid class of well log that are used to group specific curve types. Examples include gamma ray, somic, density, calliper, neutron, conductivity, formation density etc.','USER','dbo','TABLE','WELL_LOG_CURVE_CLASS';
go

execute sp_addextendedproperty 'Description','WELL LOG CURVE INDEX (OR FRAME): The term Frame has two closely related meanings depending on its context. A "Frame of Data" contains one sample of each curve associated with a specific primary index value (e.g. depth or time). In this case, a sample maybe a single value, or a complex multi dimensional array. The primary index is most commonly depth or time, but may be anything. The "Frame Specification" specifies which curves are to be grouped together, the type of the common index (depth, time, etc.), and the sampling characteristics (regular, irregular, spacing between samples if regular, etc.) The only digital log data format which specifically exposes frames is DLIS. The other formats (LIS, LAS, BIT, etc.) all use frames, but since there is only ever one Frame Specification per File, it is often lumped in with the information about theFile. DLIS, on the other hand, provides for multiple Frame Specifications (and consequently instances of each type of frame specified). This information is important, and must be retained in any system which tracks information about DLIS tapes or files. (Technically, LIS provides a mechanism forrecording multiple frame types per File, but this has never been utilized and has therefore been ignored.)','USER','dbo','TABLE','WELL_LOG_CURVE_FRAME';
go

execute sp_addextendedproperty 'Description','WELL LOG CURVE PROCESSES: This table contains a list of the processes that have been applied to a curve, usually shown as a set of processing indicators.','USER','dbo','TABLE','WELL_LOG_CURVE_PROC';
go

execute sp_addextendedproperty 'Description','WELL LOG CURVE REMARK: Use this table to capture narrative comments about a well log curve.','USER','dbo','TABLE','WELL_LOG_CURVE_REMARK';
go

execute sp_addextendedproperty 'Description','WELL LOG CURVE SCALE: The Well Log Curve Scale table records information about the scale or calibration for the well log curve or trace associated with the logging operation.','USER','dbo','TABLE','WELL_LOG_CURVE_SCALE';
go

execute sp_addextendedproperty 'Description','WELL LOG CURVE SPLICE: This table is used to track how composite logs are generated. A composite log may be comprised of sections of other log files, derived through the database or referenced in the records management system.','USER','dbo','TABLE','WELL_LOG_CURVE_SPLICE';
go

execute sp_addextendedproperty 'Description','WELL LOG DIGITAL VALUES: This table may be used to capture actual digital log curve values if desired.','USER','dbo','TABLE','WELL_LOG_CURVE_VALUE';
go

execute sp_addextendedproperty 'Description','WELL LOG DIGIT CURVE: The Well Log Digit Curve table records the digital information associated with each individual well log curve or trace.','USER','dbo','TABLE','WELL_LOG_DGTZ_CURVE';
go

execute sp_addextendedproperty 'Description','WELL LOG DICTIONARY: The dictionary contains a set of curve names, property names and parameters that are used by a well logging contracter during a specified period of time. At any given time, a contractor may have one or more dictionaries in use.','USER','dbo','TABLE','WELL_LOG_DICTIONARY';
go

execute sp_addextendedproperty 'Description','WELL LOG DICTIONARY ALIAS: all names, codes and identifiers for a well logging dictionary may be loaded into this table. Preferred versions can be denormalized into the WELL LOG DICTIONARY table if desired.','USER','dbo','TABLE','WELL_LOG_DICT_ALIAS';
go

execute sp_addextendedproperty 'Description','WELL LOG DICTIONARY BUSINESS ASSOCIATE: This table is used to list the companies that use a well log dictionary over a period of time.','USER','dbo','TABLE','WELL_LOG_DICT_BA';
go

execute sp_addextendedproperty 'Description','WELL LOG DICTIONARY CURVE CLASS: This table provides classification for well curve types defined in a dictionary. Each curve may belong to one or more classes.','USER','dbo','TABLE','WELL_LOG_DICT_CRV_CLS';
go

execute sp_addextendedproperty 'Description','WELL LOG DICTIONARY CURVE: A list of the curves that are generally available from a logging contractor, and listed in a specific dictionary.','USER','dbo','TABLE','WELL_LOG_DICT_CURVE';
go

execute sp_addextendedproperty 'Description','WELL LOG DICTIONARY PARAMETERS: A list of parameters that are used in a particular dictionary.','USER','dbo','TABLE','WELL_LOG_DICT_PARM';
go

execute sp_addextendedproperty 'Description','WELL LOG DICTIONARY PARAMETER CLASS: indicates which dictionaries a parameter classification belongs to.','USER','dbo','TABLE','WELL_LOG_DICT_PARM_CLS';
go

execute sp_addextendedproperty 'Description','WELL LOG PARAMETER VALUE: this table may be used to store recommended values for certain parameters as provided by the creators of a dictionary. For example, borehole status may be CASED or OPEN.','USER','dbo','TABLE','WELL_LOG_DICT_PARM_VAL';
go

execute sp_addextendedproperty 'Description','DICTIONARY WELL LOG PROCESSES: this table contains a list of all the processes that can be applied to a curve by the logging company, such as averaged, derived, spliced, pressure corrected, normalized. Part of RP66.','USER','dbo','TABLE','WELL_LOG_DICT_PROC';
go

execute sp_addextendedproperty 'Description','WELL LOG JOB: A Job encompasses all of the activities performed by a Business Entity (generally a Service Company), while it is engaged by the Operator of the Well to perform services. The scope of services for the Job is generally specified under the terms of a contract or service order. The Job begins when the Service Company arrives at the Well and ends when it leaves. As an example, for a land based well, a Job begins when the Logging Crew arrives in the truck at the well site and it ends when they drive away.','USER','dbo','TABLE','WELL_LOG_JOB';
go

execute sp_addextendedproperty 'Description','WELL LOG PARAMETER: Use this table to track the parameters of the well log, such as well name, surface temperature etc. Many of these attributes can also be mapped into the PPDM model if desired. Store here if you wish to manage data on an as reported basis with each log.','USER','dbo','TABLE','WELL_LOG_PARM';
go

execute sp_addextendedproperty 'Description','WELL LOG PARAMETER ARRAY: Array parameters have become more prevalent with the advent of DLIS. These most often occur in logging tool or processing related parameters. An array of weighting values used in a digital filtering may be a reasonable example. Itwould have a single name, with multiple associated values.','USER','dbo','TABLE','WELL_LOG_PARM_ARRAY';
go

execute sp_addextendedproperty 'Description','WELL LOG PARAMETER CLASS: Classifications for well log parameters, allowing similar parameters to be sorted into logical groups. For example, a class of parameters may be well name, for this class many different parameter mnemonics may be found.','USER','dbo','TABLE','WELL_LOG_PARM_CLASS';
go

execute sp_addextendedproperty 'Description','WELL LOG PASS: Pass - A Pass is any continuous recording of sensor readings for the logging instruments within a Trip. A Pass begins when data recording is started and ends when data recording is stopped. For depth based data acquisition, the Tool String is generally moving up or down the Well borehole during a Pass, whereas it may be stationary for time based data acquisition. Passes exist within the context of a Trip and there may be 0, 1, or more Passes per Trip.','USER','dbo','TABLE','WELL_LOG_PASS';
go

execute sp_addextendedproperty 'Description','WELL LOG REMARK: Use this table to capture narrative comments about a well log.','USER','dbo','TABLE','WELL_LOG_REMARK';
go

execute sp_addextendedproperty 'Description','WELL LOG TRIP: A Trip encompasses all of the activities performed by a Business Entity (generally a Service Company), while a particular Logging Tool String is in the Well borehole. The Trip begins when the tool is inserted into the hole and end when it is pulled out. Trips exist within the context of a Run and there may be 0, 1, or more Trips per Run.','USER','dbo','TABLE','WELL_LOG_TRIP';
go

execute sp_addextendedproperty 'Description','WELL LOG TRIP REMARK: This table contains narrative or descriptive remarks about a well log trip.','USER','dbo','TABLE','WELL_LOG_TRIP_REMARK';
go

execute sp_addextendedproperty 'Description','WELL MISCELLANEOUS DATA: Use this vertical table to capture information about a well that is not managed by other tables or columns in the model. Check with PPDM experts before using this column, and submit recommendations for change to the PPDM Association.','USER','dbo','TABLE','WELL_MISC_DATA';
go

execute sp_addextendedproperty 'Description','WELL MUD PROPERTY: Use this table to describe the properties of well mud','USER','dbo','TABLE','WELL_MUD_PROPERTY';
go

execute sp_addextendedproperty 'Description','WELL MUD RESISTIVITY: The Mud Resistivity table contains information pertaining to the resistivity found in mud, mud cake or filtrate. It is used to determine the nature of the strata penetrated indicating the content of gas, oil, or water enclosed in rock pores: formations containing salt water have low resistivity, those containing freshwater or oil have higher resistivities.','USER','dbo','TABLE','WELL_MUD_RESISTIVITY';
go

execute sp_addextendedproperty 'Description','WELL MUD SAMPLE: The Mud Sample table contains information on the physical, chemical, and electrical properties of the drilling fluid which is associated with a set of wireline logs from the same wireline logging job. The mud sample is a sample of the fluid thatis circulated through the wellbore during the drilling operation. It is the physical properties of the mud that have a direct affect on the measurements taken during a logging operation.','USER','dbo','TABLE','WELL_MUD_SAMPLE';
go

execute sp_addextendedproperty 'Description','NODE: a position for which a location must be permanently tracked. WELL NODE: Definition of a well point projected to the Earth"s surface with its "selected" coordinate value. SURFACE WELL NODE: a surface well position which must be kept permanently. SUBSURFACEWELL NODE: A subsurface wll location, such as a KOP which must be kept permanently.','USER','dbo','TABLE','WELL_NODE';
go

execute sp_addextendedproperty 'Description','WELL NODE AREA: this table tracks the relationships between well nodes and all areas that they intersect with. These areas may be formal geopolitical areas, business or regulatory areas, informal areas etc.','USER','dbo','TABLE','WELL_NODE_AREA';
go

execute sp_addextendedproperty 'Description','NODE METES AND BOUNDS: The Node Metes and Bounds table contains information about the metes and bounds method of describing the boundary of a tract of land. Lengths and bearings are often referenced to a local, natural, or man-made structure. The metes are measurements and the bounds are bearings. Metes and bounds were used for land surveys in the original 13 United States and Texas, as well as in Canada.','USER','dbo','TABLE','WELL_NODE_M_B';
go

execute sp_addextendedproperty 'Description','WELL NODE STRATIGRAPHIC UNIT: Using survey data gathered during drilling (MWD), and interpretation data, you can derive, at multiple points along the well path, the interpreted true vertical distance (TVD offset) between the well bore location and the top of the formation(s) of interest. A negative TVD offset indicates the top of the formation is above the well bore, a positive TVD offset indicates the top of the formation is below the well bore, and a zero TVD offset indicates the wellbore is at the top of the formation (i.e either crossing into the zone or out of it). At any given measured depth, there may be TVD offsets to one or many formations.','USER','dbo','TABLE','WELL_NODE_STRAT_UNIT';
go

execute sp_addextendedproperty 'Description','WELL NODE VERSION: Carries all the different coordinate values associated with a well point projected to the surface of the Earth. NODE VERSION: a version of a location for a position which must be kept permanently.','USER','dbo','TABLE','WELL_NODE_VERSION';
go

execute sp_addextendedproperty 'Description','WELL PAYZONE: The Well Payzone table contains information on the pay of producing zones in the well. Described is the thickness of net pay and gross pay zones within an interval of the wellbore. The top and bottom depth define the positio n of the pay interval within the wellbore.','USER','dbo','TABLE','WELL_PAYZONE';
go

execute sp_addextendedproperty 'Description','WELL PERFORATION: The Well Perforations tables contains detailed about the perforation activity performed on a well. This is the puncturing of a casing or liner which allows passage of fluid. A well completion may contain many well perfo ration.','USER','dbo','TABLE','WELL_PERFORATION';
go

execute sp_addextendedproperty 'Description','WELL PERFORATION REMARK: contains narrative remarks about the well perforation.','USER','dbo','TABLE','WELL_PERF_REMARK';
go

execute sp_addextendedproperty 'Description','REFERENCE WELL PERMIT TYPE: the type of permit that has been issued for the well, such as Rule 37 or Rule 38 in the State of Texas)..','USER','dbo','TABLE','WELL_PERMIT_TYPE';
go

execute sp_addextendedproperty 'Description','WELL PLUGBACK: The Well Plugback table contains information pertaining to a well plug back (e.g., depth, date, amount of cement, plugback type etc.). A plug-back is an operation used in a well to abandon and plug a deeper reservoir with a plugback plug and moveup the well to complete in a shallower reservoir.','USER','dbo','TABLE','WELL_PLUGBACK';
go

execute sp_addextendedproperty 'Description','WELL POROUS INTERVAL: The Well Porous Interval table describes information about the type, thickness, depth range and stratigraphic formation of rock porosity in an interval. Since rock porosity can be identified over many intervals in a wellbore, the primary key is POROUS_INTRVL_ID.','USER','dbo','TABLE','WELL_POROUS_INTERVAL';
go

execute sp_addextendedproperty 'Description','WELL PRESSURE: The Well Pressure table contains summary information about pressure tests performed on gas wells. The pressure tests may be 4 point, absolute open flow potential, or bottom hole.','USER','dbo','TABLE','WELL_PRESSURE';
go

execute sp_addextendedproperty 'Description','WELL ABSOLUTE OPEN FLOW PRESSURE: The Well Absolute Open Flow Pressure table contains absolute open flow well pressure information pertaining to the production of oil or gas under wide-open conditions (the flow of production from a well wi thout any restrictions on the rate of flow).','USER','dbo','TABLE','WELL_PRESSURE_AOF';
go

execute sp_addextendedproperty 'Description','WELL PRESSURE AOF 4 POINT: The Well Pressure 4 Point table contains fl ow rate and pressure information pertaining to various changes of choke size during a four point test. Each point corresponds to a change in the choke size. This data i s obtained during production tests.','USER','dbo','TABLE','WELL_PRESSURE_AOF_4PT';
go

execute sp_addextendedproperty 'Description','WELL BOTTOM HOLE PRESSURE: The Well Bottom Hole Pressure table contains information pertaining to the reservoir or formation pressure at the bottom of the hole. If measured under flowing conditions, readings are usually taken at different rates of flow in order to arrive at a maximum productivity rate. A decline in pressure indicates the amount of depletion from the reservoir.','USER','dbo','TABLE','WELL_PRESSURE_BH';
go

execute sp_addextendedproperty 'Description','WELL REMARK: The Well Remark table contains additional information pertaining to a specific event within the wellbore. Remarks contained in this table may also refer to information regarding formation and stratigraphic interval referenced by remarks.','USER','dbo','TABLE','WELL_REMARK';
go

execute sp_addextendedproperty 'Description','WELL SET: A Well Set is a grouping mechanism for Well Components used to maintain and end to end link through all stages of the Well Life cycle (planning to disposal).','USER','dbo','TABLE','WELL_SET';
go

execute sp_addextendedproperty 'Description','WELL SET WELL: A Well Set is a grouping mechanism for Well Components used to maintain and end to end link through all stages of the Well Life cycle (planning to disposal). Use this table to associate the appropriate well components with a set.','USER','dbo','TABLE','WELL_SET_WELL';
go

execute sp_addextendedproperty 'Description','WELL SHOW: The Well Show table contains information about the appearance of oil and/or gas obtained from a sample type (e.g., cuttings, pit, bailer etc.). Description of the lithology associated with the hydrocarbon show is also contained within this table.','USER','dbo','TABLE','WELL_SHOW';
go

execute sp_addextendedproperty 'Description','WELL SHOW REMARK: contains narrative remarks about the well show.','USER','dbo','TABLE','WELL_SHOW_REMARK';
go

execute sp_addextendedproperty 'Description','Z WELL SIEVE ANALYSIS (to be deprecated): The Well Sieve Analysis table contains information identifying a specific location in a wellbore where samples for a sieve analysis were obtained.  A sieve analysis determines the particle-size distribution in soil, se  diment, or rock by measuring the percentage of the particles that pass through standard sieves of various sizes. THIS TABLE IS SCHEDULE TO BE DEPRECATED, PLEASE USE THE ANALYSIS SUBJECT AREA.','USER','dbo','TABLE','WELL_SIEVE_ANALYSIS';
go

execute sp_addextendedproperty 'Description','WELL SIEVE SCREEN: The Well Sieve Screen table contains information relating to the sieve screen mesh size. This apparatus is used to separate soil or sedimentary material according to the size of its particles: it is usually made of brass with a wire-mesh cloth, having regularly spaced square holes of uniform diameters, spread across the base.','USER','dbo','TABLE','WELL_SIEVE_SCREEN';
go

execute sp_addextendedproperty 'Description','WELL STATUS: The Well Status table contains an historical account of the operating status of the wellbore. Described is the actual status of the wellbore on a specific date. The information may include the dates when the well was completed , produced oil/gas, injected, or was plugged or abandoned.','USER','dbo','TABLE','WELL_STATUS';
go

execute sp_addextendedproperty 'Description','WELL SUPPORT FACILITY: Use this table to associate wells with the support facilities used during construction and operations. Common uses include details about RIGS used during construction or recompletion, WASTE DISPOSAL facilities, HABITATS etc.','USER','dbo','TABLE','WELL_SUPPORT_FACILITY';
go

execute sp_addextendedproperty 'Description','WELL TEST: The Well Test table contains descriptive information concerning a test date, depth interval, the objective stratigraphic unit, and other general data for a DST (Drill Stem Test) and productivity test. These tests are used as an evaluation tool to help determine the potential of the reservoir without completing the well.','USER','dbo','TABLE','WELL_TEST';
go

execute sp_addextendedproperty 'Description','WELL TEST ANAYSIS: The Well Test Analysis table contains descriptive information pertaining to an analysis which has been conducted on fluids recovered during a well test. A wealth of information may be obtained through a well test analys is such as GOR (the ratio of gas to oil), percentage of hydrogen sulfide, water resistivity, or salinity.','USER','dbo','TABLE','WELL_TEST_ANALYSIS';
go

execute sp_addextendedproperty 'Description','WELL TEST COMPUTED ANALYSIS: The Well Test Computed Analysis table contains computed analysis information obtained from testing laboratories (e.g., measurement of permeability of the formation test tested, the production index (flow rate a well can produce), estimated damage ratio and maximum reservoir pressure computed from plotted gauge information.','USER','dbo','TABLE','WELL_TEST_COMPUT_ANAL';
go

execute sp_addextendedproperty 'Description','WELL TEST CONTAMINANT: The Well Test Contaminant table contains information pertaining to the type of contaminant encountered during a well test, such as corrosive gasses.','USER','dbo','TABLE','WELL_TEST_CONTAMINANT';
go

execute sp_addextendedproperty 'Description','WELL TEST CUSHION: The Well Test Cushion table records descriptive information about the type and amount of cushion used during a drill stem test to buffer the formation being tested from the effects of radical pressure change.','USER','dbo','TABLE','WELL_TEST_CUSHION';
go

execute sp_addextendedproperty 'Description','WELL TEST EQUIPMENT: The Well Test Equipment table contains descriptive information about the specific tools used to perform a well test. Included are the type of tool used (EQPT_TYPE) and the specific feature of that tool (e.g., length, weight, and diameter).','USER','dbo','TABLE','WELL_TEST_EQUIPMENT';
go

execute sp_addextendedproperty 'Description','WELL TEST FLOW: The Well Test Flow table provides information about the flow of material recovered at the surface. It also describes pressure associated with surface recoveries during each well test period.','USER','dbo','TABLE','WELL_TEST_FLOW';
go

execute sp_addextendedproperty 'Description','WELL TEST FLOW RATE: The Well Test Flow Rate table provides information unique to each flow rate conducted for a well test such as surface choke diameter, flow rate pressure, and time.','USER','dbo','TABLE','WELL_TEST_FLOW_MEAS';
go

execute sp_addextendedproperty 'Description','WELL TEST MUD: The Well Test Mud table contains information on the physical, chemical and electrical properties of the drilling fluid associated with a well test.','USER','dbo','TABLE','WELL_TEST_MUD';
go

execute sp_addextendedproperty 'Description','WELL TEST PERIOD: The Well Test Period table contains information about the individual, discrete events or periods during a well test. Most of these periods occur after the packer or packers are set to isolate the zone from the drilling fl uid column.','USER','dbo','TABLE','WELL_TEST_PERIOD';
go

execute sp_addextendedproperty 'Description','WELL TEST PRESSURE: The Well Test Pressure table contains information related to a unique observation associated with a pressure for each well test period. Pressure surveys are recorded from one or more pressure gauges used in a well test.','USER','dbo','TABLE','WELL_TEST_PRESSURE';
go

execute sp_addextendedproperty 'Description','WELL TEST INCREMENT: The Well Test Increment table contains information unique to each well test time increment. It provides the increment pressure, the increment temperature and increment time referenced in the well test period.','USER','dbo','TABLE','WELL_TEST_PRESS_MEAS';
go

execute sp_addextendedproperty 'Description','WELL TEST RECORDER: The Well Test Recorder table contains computed analysis gauge information obtained from gauge reports, such as recorder type, position, resolution, or depth etc.','USER','dbo','TABLE','WELL_TEST_RECORDER';
go

execute sp_addextendedproperty 'Description','WELL TEST RECOVERY: The Well Test Recovery table contains information about the fluids recovered from the drill pipe as result of running a well test.','USER','dbo','TABLE','WELL_TEST_RECOVERY';
go

execute sp_addextendedproperty 'Description','WELL TEST REMARK: The Well Test Remark table consists of additional descriptive comments about the well test, including comments regarding any problem affecting the results, such as misrun due to packer failure, or shut-in valve leaking.','USER','dbo','TABLE','WELL_TEST_REMARK';
go

execute sp_addextendedproperty 'Description','WELL TEST SHUTOFF: The Well Test Shutoff table contains information describing the type of shutoff method used to shut off perforated or openhole intervals. This data should reflect those intervals that are shut off at the time the well u nit is completed.','USER','dbo','TABLE','WELL_TEST_SHUTOFF';
go

execute sp_addextendedproperty 'Description','WELL TEST FORMATION: This table describes the well test overlap formations.','USER','dbo','TABLE','WELL_TEST_STRAT_UNIT';
go

execute sp_addextendedproperty 'Description','WELL TREATMENT: The Well Treatment table contains information describing the stimulation treatment performed during the life of the well. Common types of treatment include hydraulic fracturing and the acidizing jobs. A wellbore may have many treatment operations, therefore, the table is uniquely identified by the specific interval and treatment type.','USER','dbo','TABLE','WELL_TREATMENT';
go

execute sp_addextendedproperty 'Description','WELL TUBULAR: The Well Tubular table contains information on the tubulars for the well. The tubulars can be tubing, casing or liners which are run into the well. A tubing string is a continuous length of pipe used to connect the producing interval in the well tothe flowline at the surface. A casing string is a continuous length of pipe used to protect the walls of the well and to assist in interval isolation when completing the well. The table identifies each tubular stri ng used in a single drilling phase of the well. Liners are used to prevent sand from entering the well bore.','USER','dbo','TABLE','WELL_TUBULAR';
go

execute sp_addextendedproperty 'Description','WELL VERSION: This table is a duplicate of the WELL table with the addition of SOURCE in the primary key to accommodate versioning. Different sources may report different values for the same data item, due to reliability of collection method or source document, time (age) of the report, etc.','USER','dbo','TABLE','WELL_VERSION';
go

execute sp_addextendedproperty 'Description','WELL VERSION AREA: identifies any areas within a WELL version. May be geopolitical, regulatory, formal, informal etc.','USER','dbo','TABLE','WELL_VERSION_AREA';
go

execute sp_addextendedproperty 'Description','WELLCROSS REFERENCE: Supports complex relationships between wells. For example, a planned well and the actual well that was drilled, or more complex relationships between a parent well and well bores, re-entries, such as re-completions, side bores etc can all be captured.','USER','dbo','TABLE','WELL_XREF';
go

execute sp_addextendedproperty 'Description','WELL ZONE INTERVAL: The Well Zone Interval table contains information within a wellbore pertaining to a belt, layer, band, or strip of earth materials (e.g., rock or soil), disposed horizontall y, vertically, concentrically, or otherwise, and characterized as distinct from surrounding parts by some particular property or content (e.g. part of the Earth"s crust such a s a zone of saturation or the zone of fracture, or a structural zone characterized by folding of different styles and periods.','USER','dbo','TABLE','WELL_ZONE_INTERVAL';
go

execute sp_addextendedproperty 'Description','WELL ZONE INTERVAL VALUE: The Well Zone Interval Value table contains the actual data values as a result of an interpretation from a wellbore zone interval.','USER','dbo','TABLE','WELL_ZONE_INTRVL_VALUE';
go

execute sp_addextendedproperty 'Description','WORK ORDER: a work order represents a request or authorization for work to be performed.','USER','dbo','TABLE','WORK_ORDER';
go

execute sp_addextendedproperty 'Description','WORK ORDER ALIAS: Alternate names and identifiers for a work order.','USER','dbo','TABLE','WORK_ORDER_ALIAS';
go

execute sp_addextendedproperty 'Description','WORK ORDER BUSINESS ASSOCIATE: This table tracks information about all the parties that are involved with a work order, from clients to other service providers. May be requestor, destination, billing etc.','USER','dbo','TABLE','WORK_ORDER_BA';
go

execute sp_addextendedproperty 'Description','WORK ORDER COMPONENT: this table is used to associate the work order with all business components of the model that are related to the work order, such as information items, seismic sets, AFEs, contracts etc.','USER','dbo','TABLE','WORK_ORDER_COMPONENT';
go

execute sp_addextendedproperty 'Description','WORK ORDER CONDITION: This table tracks some information about conditions in a work order, such as payments.','USER','dbo','TABLE','WORK_ORDER_CONDITION';
go

execute sp_addextendedproperty 'Description','WORK ORDER DELIVERY: This table is used to track delivery of products from the work order. The subordinate table, WORK ORDER DELIVERY COMP, tracks the individual products in the delivery.','USER','dbo','TABLE','WORK_ORDER_DELIVERY';
go

execute sp_addextendedproperty 'Description','WORK ORDER DELIVERY COMPONENT: This table tracks the delivery of specific components with a work order. Note that the meaning of the term DELIVERY varies according to the value in DELIVERY TYPE in the parent table.','USER','dbo','TABLE','WORK_ORDER_DELIVERY_COMP';
go

execute sp_addextendedproperty 'Description','WORK ORDER INSTRUCTIONS: This table captures all of the instructions provided with a work order, including details about what is to be done, where products are to be shipped and so on.','USER','dbo','TABLE','WORK_ORDER_INSTRUCTION';
go

execute sp_addextendedproperty 'Description','WORK ORDER INSTRUCTION COMPONENT: This table allows each compoonent of a work order to be explicitly associated with one or more instructions.','USER','dbo','TABLE','WORK_ORDER_INST_COMP';
go

execute sp_addextendedproperty 'Description','WORK ORDER CROSS REFERENCE: a cross refernce table capturing the relationships between work orders. These relationships may be historical (work order replaces another), functional (this work order was divided into two subordinate work orders) or transactional (company A sent a work order which was used to create a new work order).','USER','dbo','TABLE','WORK_ORDER_XREF';
go

execute sp_addextendedproperty 'Description','ZONE: The ZONE table contains the name of a zone described within a wellbore. A zone may be a regular or irregular belt, layer, band, or strip of earth materials disposed horizontally, vertically, concentrically or otherwise, and character ized as distinct from surrounding parts by some particular property or content.','USER','dbo','TABLE','ZONE';
go

execute sp_addextendedproperty 'Description','PRODUCT TYPE: A table identifying the type of product (fluid) such as GAS, OIL, WATER, NGL, etc. Includes the less common products like STEAM, METHANE, BUTANE, HELIUM, etc.','USER','dbo','TABLE','Z_PRODUCT';
go

execute sp_addextendedproperty 'Description','PRODUCT COMPOSITION: This table is used to describe the subproducts into which products can be broken down.','USER','dbo','TABLE','Z_PRODUCT_COMPOSITION';
go

execute sp_addextendedproperty 'Description','OIL BASE TYPE: The type of oil base used in the fluid analysis.','USER','dbo','TABLE','Z_R_OIL_BASE_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE OIL TYPE: This reference table identifies the type of oil which is being analyzed. For example, asphalt, paraffin.','USER','dbo','TABLE','Z_R_OIL_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE SAMPLE WATER TYPE: The type of water sample that is analysed, such as injection water, disposal water.','USER','dbo','TABLE','Z_R_SAMPLE_WATER_TYPE';
go

execute sp_addextendedproperty 'Description','REFERENCE WATER TYPE: This reference table identifies the type of water being analyzed. For example, fresh water, salt water or brackish water.','USER','dbo','TABLE','Z_R_WATER_TYPE';
go
