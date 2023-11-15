using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Runtime.Intrinsics.X86;
using System.Security.Policy;
using System.ServiceModel.Channels;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
using IOSAS.Infrastructure.WebAPI.Models;
using IOSAS.Infrastructure.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace IOSAS.Infrastructure.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly ID365WebAPIService _d365webapiservice;

        public ApplicationController(ID365WebAPIService d365webapiservice)
        {
            _d365webapiservice = d365webapiservice ?? throw new ArgumentNullException(nameof(d365webapiservice));

        }

        [HttpGet("GetById")]
        public ActionResult<string> Get(string id, string userId)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Invalid Request - Id is required");

            if (string.IsNullOrEmpty(userId))
                return BadRequest("Invalid Request - userId is userId");

            var fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                                <entity name='iosas_application'>
                                    <attribute name='iosas_name' />   
                                    <attribute name='iosas_designatedcontact' />
                                    <attribute name='iosas_expressionofinterest' />
                                    <attribute name='iosas_studentshousedinadormitorysetting' />  
                                    <attribute name='iosas_confirmationofwatertesting' />  
                                    <attribute name='iosas_emergencydrillspolicyproceduresresponse' />  
                                    <attribute name='iosas_confirmationofleadtestingfordrinkingwater' />  
                                    <attribute name='iosas_proofofleasepurchaseoffacility' />  
                                    <attribute name='iosas_numberofstudentsg6' />  
                                    <attribute name='iosas_applicationid' />  
                                    <attribute name='iosas_teacherandprincipalevaluationpolicy' />  
                                    <attribute name='iosas_acknowledgementofinterviewdecisionsent' />  
                                    <attribute name='iosas_writtenconfirmationofeligibility' />  
                                    <attribute name='iosas_hasthesaobtainedirrevocableloc' />  
                                    <attribute name='iosas_applicationnumber' />  
                                    <attribute name='iosas_primaryschooltotal' />  
                                    <attribute name='iosas_hastheauthoritycontactedlocalgovernment' />  
                                    <attribute name='iosas_feerefundpolicyconsistentwithguidelines' />  
                                    <attribute name='iosas_proofoffacilityinspection' />  
                                    <attribute name='iosas_willsaoperateonnonprofitbasis' />  
                                    <attribute name='iosas_municipalcomplianceletter' />  
                                    <attribute name='iosas_authorityheadlastname' />  
                                    <attribute name='iosas_businessplanincludingfinancialinformation' />
                                    <attribute name='iosas_schoolclosurepolicy' />  
                                    <attribute name='iosas_authorityheadphone' />  
                                    <attribute name='iosas_numberofstudentskindergarten' />
                                    <attribute name='iosas_familiarwithgrantstoispolicyifseekingfunds' />
                                    <attribute name='iosas_proponentspreviouslyinvolvedinisbc' />
                                    <attribute name='iosas_postalcode' />
                                    <attribute name='iosas_authorityprovince' />
                                    <attribute name='iosas_proposedschoolname' />
                                    <attribute name='iosas_studentshousedinahomesetting' />
                                    <attribute name='iosas_country' />
                                    <attribute name='iosas_cantheauthorityconfirmtestingvspolicy' />
                                    <attribute name='iosas_studentsupervisionpolicies' />
                                    <attribute name='iosas_policyincludessectionsdealingwithemergencies' />
                                    <attribute name='iosas_authorityaddressline1' />
                                    <attribute name='iosas_edu_schoolauthority' />
                                    <attribute name='iosas_awareoftherequirementsforcrchecks' />
                                    <attribute name='iosas_willhavecompletioncertificatepolicy' />
                                    <attribute name='iosas_preexistingauthorityhead' />
                                    <attribute name='iosas_authoritycomplieswithisaregulations' />
                                    <attribute name='iosas_nopromotionofinappropriatedoctrines' />
                                    <attribute name='iosas_numberofstudentsg5' />
                                    <attribute name='iosas_numberofstudentsg4' />
                                    <attribute name='iosas_numberofstudentsg7' />
                                    <attribute name='iosas_completesetofpoliciesoutlinedinchecklist' />
                                    <attribute name='iosas_numberofstudentsg1' />
                                    <attribute name='iosas_numberofstudentsg2' />
                                    <attribute name='iosas_highschooltotal' />
                                    <attribute name='iosas_numberofstudentsg9' />
                                    <attribute name='iosas_numberofstudentsg8' />
                                    <attribute name='iosas_studentconductstandardsdisciplinepolicy' />
                                    <attribute name='iosas_childabusepreventionpolicy' />
                                    <attribute name='iosas_schoolauthority' />
                                    <attribute name='iosas_edu_year' />
                                    <attribute name='iosas_refundpolicyconsistentwiththeguidelines' />
                                    <attribute name='iosas_seekgroup1classification' />
                                    <attribute name='iosas_authorityheadfirstname' />
                                    <attribute name='iosas_policyincludepermanentschoolclosure' />
                                    <attribute name='iosas_schoolauthorityhead' />
                                    <attribute name='iosas_studentrecordspolicy' />
                                    <attribute name='iosas_numberofstudentsg3' />
                                    <attribute name='iosas_interviewpassedsuccessfully' />
                                    <attribute name='iosas_appealsprocesspolicy' />
                                    <attribute name='iosas_submissionmethod' />
                                    <attribute name='iosas_authoritycity' />
                                    <attribute name='iosas_educationalresourcepolicy' />
                                    <attribute name='iosas_contactinformationfortwobusinessreferences' />
                                    <attribute name='iosas_preexistingauthority' />
                                    <attribute name='iosas_businessplansubmitted' />
                                    <attribute name='iosas_approvedforinterimcertification' />
                                    <attribute name='iosas_studentsafetypolicies' />
                                    <attribute name='iosas_authoritycountry' />
                                    <attribute name='iosas_startgrade' />
                                    <attribute name='iosas_anaphylaxispolicyandprocedures' />
                                    <attribute name='iosas_learningassistanceforspecialstudents' />
                                    <attribute name='iosas_acknowledgementofreceiptsent' />
                                    <attribute name='iosas_otheremergencydrillsimplemeneted' />
                                    <attribute name='iosas_authoritypostalcode' />
                                    <attribute name='iosas_addressline1' />
                                    <attribute name='iosas_acknowledgementofdecisionsent' />
                                    <attribute name='iosas_numberofstudentsg11' />
                                    <attribute name='iosas_numberofstudentsg12' />
                                    <attribute name='iosas_reviewpassedsuccessfully' />
                                    <attribute name='iosas_totalenrolment' />
                                    <attribute name='iosas_numberofstudentsg10' />
                                    <attribute name='iosas_province' />
                                    <attribute name='iosas_harassmentandbullyingpreventionpolicy' />
                                    <attribute name='iosas_privacypolicy' />
                                    <attribute name='iosas_policysubmitted' />
                                    <attribute name='iosas_groupclassification' />
                                    <attribute name='iosas_city' />
                                    <attribute name='iosas_designatedcontactsameasauthorityhead' />
                                    <attribute name='iosas_submissiondate' />
                                    <attribute name='iosas_authorityheademail' />
                                    <attribute name='iosas_irrevocableletterofcreditorsuretybond' />
                                    <attribute name='iosas_willcomplywithenactmentsofbc' />
                                    <attribute name='iosas_hassaobtaineddocumentsregardingbondingreqs' />
                                    <attribute name='iosas_precertsubmissionreviewdate' />
                                    <attribute name='iosas_testingdrinkingwaterforleadcontentpolicy' />
                                    <attribute name='iosas_interimcertificationapprovaldate' />
                                    <attribute name='iosas_hastheauthoritydevelopedarefundpolicy' />
                                    <attribute name='iosas_website' />
                                    <attribute name='iosas_describefamiliaritywithbcscurriculum' />
                                    <attribute name='iosas_awareofcertificationrequirements' />
                                    <attribute name='iosas_willapplyforstudentgraduationcreditpolicy' />
                                    <attribute name='iosas_howwillyouexercisegovernanceduties' />
                                    <attribute name='iosas_specialeducationpolicy' />
                                    <attribute name='iosas_grades112proposedhoursperday_d' />
                                    <attribute name='iosas_ifnotoanyschoolpolicyexplainwhy' />
                                    <attribute name='iosas_authoritycode' />
                                    <attribute name='iosas_proofoffacilityinspectionreceiveddate' />
                                    <attribute name='iosas_schoolaffiliation' />
                                    <attribute name='iosas_dateoflastannualreport' />
                                    <attribute name='iosas_semestertype' />
                                    <attribute name='iosas_graduationprogramcreditspolicyifapplicable' />
                                    <attribute name='iosas_halfdaykindergartenproposedhoursperday_d' />
                                    <attribute name='iosas_authoritywebsite' />
                                    <attribute name='iosas_halfdaykindergartenproposedhoursperyear_d' />
                                    <attribute name='iosas_complywithhomestayguidelines' />
                                    <attribute name='iosas_edu_school' />
                                    <attribute name='iosas_mailingaddresscity' />
                                    <attribute name='iosas_fulldaykgtproposeddaysinsesssion_d' />
                                    <attribute name='iosas_proofofleasereceiveddate' />
                                    <attribute name='iosas_homeschoolingpolicyifapplicable' />
                                    <attribute name='iosas_addressline2' />
                                    <attribute name='iosas_authorityaddressline2' />
                                    <attribute name='iosas_fulldaykgtproposedhoursperyear_d' />
                                    <attribute name='iosas_mailingaddressprovince' />
                                    <attribute name='iosas_internationalstudentpoliciesifapplicable' />
                                    <attribute name='iosas_interviewadditionalinformation' />
                                    <attribute name='iosas_numberofteachers' />
                                    <attribute name='iosas_additionalprogramsother' />
                                    <attribute name='iosas_whatstepsareyoutakingtoacquirethefacility' />
                                    <attribute name='iosas_boardingsafetyandsupervisionpolicy' />
                                    <attribute name='iosas_grades112proposedhoursperyear_d' />
                                    <attribute name='iosas_grades112proposeddaysinsession_d' />
                                    <attribute name='iosas_detailsofinvolvement' />
                                    <attribute name='iosas_officialregistrationnumber' />
                                    <attribute name='iosas_mailingaddress2' />
                                    <attribute name='iosas_mailingaddress1' />
                                    <attribute name='iosas_nameofmunicipalityorregionaldistrict' />
                                    <attribute name='iosas_schoolassociationother' />
                                    <attribute name='iosas_mailingaddresscountry' />
                                    <attribute name='iosas_halfdaykgtproposeddaysinsession_d' />
                                    <attribute name='iosas_incorporationtype' />
                                    <attribute name='iosas_mailingaddresspostalcode' />
                                    <attribute name='iosas_mailingaddresspostalcode' />
                                    <attribute name='iosas_fulldaykgtproposedhoursperday_d' />
                                    <attribute name='iosas_overview' />
                                    <attribute name='createdon' />
                                    <attribute name='modifiedon' />
                                    <attribute name='statecode' />
                                    <attribute name='statuscode' />
                                    <attribute name='createdby' />
                                    <attribute name='modifiedby' />
                                    <attribute name='iosas_portalapplicationstep' />
                                    <attribute name='iosas_endgrade' />
                                    <attribute name='iosas_phonenumber' />
                                    <attribute name='iosas_additionalprograms' />
                                    <attribute name='iosas_willdevelopbudgetforexpenditurebasedoniep'  />
                                    <attribute name='iosas_choicefieldswithnoselection'  />
                                    <attribute name='iosas_awareoftherequirementsforcrchecks'  />
                                    <attribute name='iosas_precertdocumentssubmitted'  />
                                    <filter type='and'>
                                        <condition attribute='iosas_applicationid' operator='eq' value='{id}' />
                                        <filter type='or'>
                                           <condition attribute='iosas_schoolauthorityhead' operator='eq' value='{userId}' />
                                           <condition attribute='iosas_designatedcontact' operator='eq' value='{userId}' />
                                        </filter>
                                    </filter>
                                </entity>
                            </fetch>";

            var message = $"iosas_applications?fetchXml=" + WebUtility.UrlEncode(fetchXml);

            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);

                if (root.Last().First().HasValues)
                {
                    return Ok(response.Content.ReadAsStringAsync().Result);
                }
                else
                {
                    return NotFound($"No Data: {id}");
                }
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");
        }

        [HttpGet("GetAllByUser")]
        public ActionResult<string> GetAllByUser(string userId)
        {

            if (string.IsNullOrEmpty(userId))
                return BadRequest("Invalid Request - userId is userId");


            var fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                                <entity name='iosas_application'>
                                    <attribute name='iosas_name' />
                                    <attribute name='iosas_applicationnumber' />  
                                    <attribute name='iosas_applicationid' />  
                                    <attribute name='iosas_proposedschoolname' />  
                                    <attribute name='iosas_groupclassification' />  
                                    <attribute name='iosas_edu_year' />
                                    <attribute name='createdon' />
                                    <attribute name='modifiedon' />
                                    <attribute name='statecode' />
                                    <attribute name='statuscode' />
                                    <attribute name='createdby' />
                                    <attribute name='modifiedby' />
                                    <attribute name='iosas_portalapplicationstep' />
                                    <filter type='and'>   
                                       <condition attribute='statuscode' operator='ne' value='100000011'/>
                                       <condition attribute='statecode' operator='eq' value='0'/>
                                       <filter type='or'>
                                           <condition attribute='iosas_schoolauthorityhead' operator='eq' value='{userId}' />
                                           <condition attribute='iosas_designatedcontact' operator='eq' value='{userId}' />
                                        </filter>
                                    </filter>
                                </entity>
                            </fetch>";

            var message = $"iosas_applications?fetchXml=" + WebUtility.UrlEncode(fetchXml);

            var response = _d365webapiservice.SendMessageAsync(HttpMethod.Get, message);
            if (response.IsSuccessStatusCode)
            {
                var root = JToken.Parse(response.Content.ReadAsStringAsync().Result);

                if (root.Last().First().HasValues)
                {
                    return Ok(response.Content.ReadAsStringAsync().Result);
                }
                else
                {
                    return Ok($"[]");
                }
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Retrieve records: {response.ReasonPhrase}");
        }


        [HttpPatch("Cancel")]
        public ActionResult<string> Cancel(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Invalid Request - Id is required");

            var value = new JObject
                        {
                            { "statuscode", 100000011},
                            { "iosas_notes", "Abandoned by applicant" }
                        };

            var statement = $"iosas_applications({id})";
            HttpResponseMessage response = _d365webapiservice.SendUpdateRequestAsync(statement, value.ToString());
            if (response.IsSuccessStatusCode)
                return Ok($"Application {id} Cancelled.");
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Update record: {response.ReasonPhrase}");
        }

        [HttpPatch("Update")]
        public ActionResult<string> Update([FromBody] dynamic value, string id, string userId, bool? submitted)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Invalid Request - Id is required");

            if (string.IsNullOrEmpty(userId))
                return BadRequest("Invalid Request - UserId is required");

            string statement = $"iosas_applications({id})";
            var response = _d365webapiservice.SendRetrieveRequestAsync($"{statement}?$select=statuscode,iosas_name", true);
            JObject body = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            if (body != null)
            {
                var statusId = body.GetValue("statuscode");
                if (int.Parse((string)statusId) != 100000001 && int.Parse((string)statusId) != 100000009)  // May hav to change
                {
                    return BadRequest($"Application {value.iosas_name} is already submitted.");
                }
            }

            var app = PrepareApp(value, submitted);
            response = _d365webapiservice.SendUpdateRequestAsync(statement, app.ToString());
            if (response.IsSuccessStatusCode)
            {
                return Ok($"Application {value.iosas_applicationid} updated successfully");
            }
            else
                return StatusCode((int)response.StatusCode,
                    $"Failed to Update record: {response.ReasonPhrase}");
        }

        private JObject PrepareApp(dynamic value, bool? submitted)
        {
            var app = new JObject();

            app["iosas_overview"] = value.iosas_overview;
            app["iosas_proposedschoolname"] = value.iosas_proposedschoolname;
            app["iosas_startgrade"] = value.iosas_startgrade;
            app["iosas_endgrade"] = value.iosas_endgrade;
            app["iosas_phonenumber"] = value.iosas_phonenumber;
            app["iosas_website"] = value.iosas_website;
            app["iosas_groupclassification"] = value.iosas_groupclassification;
            app["iosas_seekgroup1classification"] = value.iosas_seekgroup1classification;

            //multi-select
            app["iosas_schoolaffiliation"] = value.iosas_schoolaffiliation;

            app["iosas_schoolassociationother"] = value.iosas_schoolassociationother;
            app["iosas_addressline1"] = value.iosas_addressline1;
            app["iosas_addressline2"] = value.iosas_addressline2;
            app["iosas_city"] = value.iosas_city;
            app["iosas_province"] = value.iosas_province;
            app["iosas_postalcode"] = value.iosas_postalcode;
            app["iosas_country"] = value.iosas_country;

            app["iosas_mailingaddress1"] = value.iosas_mailingaddress1;
            app["iosas_mailingaddresscountry"] = value.iosas_mailingaddresscountry;
            app["iosas_mailingaddress2"] = value.iosas_mailingaddress2;
            app["iosas_mailingaddresscity"] = value.iosas_mailingaddresscity;
            app["iosas_mailingaddressprovince"] = value.iosas_mailingaddressprovince;
            app["iosas_mailingaddresspostalcode"] = value.iosas_mailingaddresspostalcode;

            app["iosas_nopromotionofinappropriatedoctrines"] = value.iosas_nopromotionofinappropriatedoctrines;
            app["iosas_willcomplywithenactmentsofbc"] = value.iosas_willcomplywithenactmentsofbc;
            app["iosas_authoritycomplieswithisaregulations"] = value.iosas_authoritycomplieswithisaregulations;

            app["iosas_howwillyouexercisegovernanceduties"] = value.iosas_howwillyouexercisegovernanceduties;
            app["iosas_proponentspreviouslyinvolvedinisbc"] = value.iosas_proponentspreviouslyinvolvedinisbc;
            app["iosas_detailsofinvolvement"] = value.iosas_detailsofinvolvement;

            app["iosas_submissionmethod"] = 100000001;
            if (submitted.HasValue)
            {
                if (submitted.Value)
                {
                    app["statuscode"] = 100000002; //New (sbumitted)
                    app["iosas_submissiondate"] = DateTime.UtcNow;
                }
                else
                {
                    app["statuscode"] = 100000001; //Draft
                }
            }

            app["iosas_DesignatedContact@odata.bind"] = $"/contacts({value._iosas_designatedcontact_value})";

            app["iosas_preexistingauthorityhead"] = value.iosas_preexistingauthorityhead;          
            if (value.iosas_preexistingauthorityhead != null && (bool)value.iosas_preexistingauthorityhead)
                app["iosas_SchoolAuthorityHead@odata.bind"] = $"/contacts({value._iosas_schoolauthorityhead_value})";
            else
                app["iosas_SchoolAuthorityHead@odata.bind"] = null;

            app["iosas_authorityheadfirstname"] = value.iosas_authorityheadfirstname;
            app["iosas_authorityheadlastname"] = value.iosas_authorityheadlastname;
            app["iosas_authorityheademail"] = value.iosas_authorityheademail;
            app["iosas_authorityheadphone"] = value.iosas_authorityheadphone;
            app["iosas_dateoflastannualreport"] = value.iosas_dateoflastannualreport;
            app["iosas_officialregistrationnumber"] = value.iosas_officialregistrationnumber;
            app["iosas_incorporationtype"] = value.iosas_incorporationtype;

            app["iosas_numberofstudentskindergarten"] = value.iosas_numberofstudentskindergarten;
            app["iosas_numberofstudentsg1"] = value.iosas_numberofstudentsg1;
            app["iosas_numberofstudentsg2"] = value.iosas_numberofstudentsg2;
            app["iosas_numberofstudentsg3"] = value.iosas_numberofstudentsg3;
            app["iosas_numberofstudentsg4"] = value.iosas_numberofstudentsg4;
            app["iosas_numberofstudentsg5"] = value.iosas_numberofstudentsg5;
            app["iosas_numberofstudentsg6"] = value.iosas_numberofstudentsg6;
            app["iosas_numberofstudentsg7"] = value.iosas_numberofstudentsg7;
            app["iosas_numberofstudentsg8"] = value.iosas_numberofstudentsg8;
            app["iosas_numberofstudentsg9"] = value.iosas_numberofstudentsg9;
            app["iosas_numberofstudentsg10"] = value.iosas_numberofstudentsg10;
            app["iosas_numberofstudentsg11"] = value.iosas_numberofstudentsg11;
            app["iosas_numberofstudentsg12"] = value.iosas_numberofstudentsg12;

            app["iosas_semestertype"] = value.iosas_semestertype;
            app["iosas_halfdaykindergartenproposedhoursperday_d"] = value.iosas_halfdaykindergartenproposedhoursperday_d;
            app["iosas_fulldaykgtproposeddaysinsesssion_d"] = value.iosas_fulldaykgtproposeddaysinsesssion_d;
            app["iosas_grades112proposedhoursperday_d"] = value.iosas_grades112proposedhoursperday_d;
            app["iosas_halfdaykgtproposeddaysinsession_d"] = value.iosas_halfdaykgtproposeddaysinsession_d;
            app["iosas_fulldaykgtproposedhoursperday_d"] = value.iosas_fulldaykgtproposedhoursperday_d;
            app["iosas_grades112proposeddaysinsession_d"] = value.iosas_grades112proposeddaysinsession_d;
            app["iosas_halfdaykindergartenproposedhoursperyear_d"] = value.iosas_halfdaykindergartenproposedhoursperyear_d;
            app["iosas_fulldaykgtproposedhoursperyear_d"] = value.iosas_fulldaykgtproposedhoursperyear_d;
            app["iosas_grades112proposedhoursperyear_d"] = value.iosas_grades112proposedhoursperyear_d;
            app["iosas_familiarwithgrantstoispolicyifseekingfunds"] = value.iosas_familiarwithgrantstoispolicyifseekingfunds;
            app["iosas_willsaoperateonnonprofitbasis"] = value.iosas_willsaoperateonnonprofitbasis;
            app["iosas_hassaobtaineddocumentsregardingbondingreqs"] = value.iosas_hassaobtaineddocumentsregardingbondingreqs;
            app["iosas_hasthesaobtainedirrevocableloc"] = value.iosas_hasthesaobtainedirrevocableloc;
            app["iosas_hastheauthoritydevelopedarefundpolicy"] = value.iosas_hastheauthoritydevelopedarefundpolicy;

            app["iosas_nameofmunicipalityorregionaldistrict"] = value.iosas_nameofmunicipalityorregionaldistrict;
            app["iosas_hastheauthoritycontactedlocalgovernment"] = value.iosas_hastheauthoritycontactedlocalgovernment;
            app["iosas_whatstepsareyoutakingtoacquirethefacility"] = value.iosas_whatstepsareyoutakingtoacquirethefacility;
            app["iosas_confirmationofleadtestingfordrinkingwater"] = value.iosas_confirmationofleadtestingfordrinkingwater;
            app["iosas_studentshousedinahomesetting"] = value.iosas_studentshousedinahomesetting;
            app["iosas_studentshousedinadormitorysetting"] = value.iosas_studentshousedinadormitorysetting;

            app["iosas_anaphylaxispolicyandprocedures"] = value.iosas_anaphylaxispolicyandprocedures;
            app["iosas_boardingsafetyandsupervisionpolicy"] = value.iosas_boardingsafetyandsupervisionpolicy;
            app["iosas_harassmentandbullyingpreventionpolicy"] = value.iosas_harassmentandbullyingpreventionpolicy;
            app["iosas_studentconductstandardsdisciplinepolicy"] = value.iosas_studentconductstandardsdisciplinepolicy;
            app["iosas_privacypolicy"] = value.iosas_privacypolicy;
            app["iosas_testingdrinkingwaterforleadcontentpolicy"] = value.iosas_testingdrinkingwaterforleadcontentpolicy;
            app["iosas_studentrecordspolicy"] = value.iosas_studentrecordspolicy;
            app["iosas_feerefundpolicyconsistentwithguidelines"] = value.iosas_feerefundpolicyconsistentwithguidelines;
            app["iosas_appealsprocesspolicy"] = value.iosas_appealsprocesspolicy;
            app["iosas_childabusepreventionpolicy"] = value.iosas_childabusepreventionpolicy;
            app["iosas_graduationprogramcreditspolicyifapplicable"] = value.iosas_graduationprogramcreditspolicyifapplicable;
            app["iosas_homeschoolingpolicyifapplicable"] = value.iosas_homeschoolingpolicyifapplicable;
            app["iosas_educationalresourcepolicy"] = value.iosas_educationalresourcepolicy;
            app["iosas_schoolclosurepolicy"] = value.iosas_schoolclosurepolicy;
            app["iosas_studentsupervisionpolicies"] = value.iosas_studentsupervisionpolicies;
            app["iosas_teacherandprincipalevaluationpolicy"] = value.iosas_teacherandprincipalevaluationpolicy;

            app["iosas_emergencydrillspolicyproceduresresponse"] = value.iosas_emergencydrillspolicyproceduresresponse;
            app["iosas_policyincludessectionsdealingwithemergencies"] = value.iosas_policyincludessectionsdealingwithemergencies;
            app["iosas_otheremergencydrillsimplemeneted"] = value.iosas_otheremergencydrillsimplemeneted;
            app["iosas_policyincludepermanentschoolclosure"] = value.iosas_policyincludepermanentschoolclosure;

            app["iosas_internationalstudentpoliciesifapplicable"] = value.iosas_internationalstudentpoliciesifapplicable;
            app["iosas_willapplyforstudentgraduationcreditpolicy"] = value.iosas_willapplyforstudentgraduationcreditpolicy;
            app["iosas_complywithhomestayguidelines"] = value.iosas_complywithhomestayguidelines;

            app["iosas_specialeducationpolicy"] = value.iosas_specialeducationpolicy;
            app["iosas_learningassistanceforspecialstudents"] = value.iosas_learningassistanceforspecialstudents;
            app["iosas_willdevelopbudgetforexpenditurebasedoniep"] = value.iosas_willdevelopbudgetforexpenditurebasedoniep;
            app["iosas_willhavecompletioncertificatepolicy"] = value.iosas_willhavecompletioncertificatepolicy;
            app["iosas_ifnotoanyschoolpolicyexplainwhy"] = value.iosas_ifnotoanyschoolpolicyexplainwhy;
            app["iosas_portalapplicationstep"] = value.iosas_portalapplicationstep;
            app["iosas_choicefieldswithnoselection"] = value.iosas_choicefieldswithnoselection;
            app["iosas_additionalprograms"] = value.iosas_additionalprograms;
            app["iosas_numberofteachers"] = value.iosas_numberofteachers;
            app["iosas_awareofcertificationrequirements"] = value.iosas_awareofcertificationrequirements;
            app["iosas_awareoftherequirementsforcrchecks"] = value.iosas_awareoftherequirementsforcrchecks;
            app["iosas_precertdocumentssubmitted"] = value.iosas_precertdocumentssubmitted;
            app["iosas_describefamiliaritywithbcscurriculum"] = value.iosas_describefamiliaritywithbcscurriculum;
            app["iosas_additionalprogramsother"] = value.iosas_additionalprogramsother;

            return app;

        }
    }
}
