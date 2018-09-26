using Commands;
using CouchDB.Client;
using IronPython.Hosting;
using IronPython.Runtime;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PyGoogleImg.Commands
{
    public class GoogleSearchHandler : RequestHandler<GoogleSearch>
    {
        protected override void Handle(GoogleSearch request)
        {
            var _engine = Python.CreateEngine();
            var searchPaths = _engine.GetSearchPaths();
            searchPaths.Add(@"C:\Python27\Lib");
            searchPaths.Add(@"C:\Python27\Lib\site-packages");
            _engine.SetSearchPaths(searchPaths);
            var _scope = _engine.CreateScope();

            List pythonResult = _engine.Execute(

$@"from bs4 import BeautifulSoup
import requests
import re
import urllib2
import os
import argparse
import sys
import json
from sys import argv
 
# adapted from http://stackoverflow.com/questions/20716842/python-download-images-from-google-image-search

def get_soup(url,header):
    return BeautifulSoup(urllib2.urlopen(urllib2.Request(url,headers=header)),'html.parser')

def main(args):
	parser = argparse.ArgumentParser(description='Scrape Google images')
	parser.add_argument('-s', '--search', default='{request.Search}', type=str, help='search term')
	args = parser.parse_args()
	query = args.search#raw_input(args.search)
	image_type='Action'
	query= query.split()
	query='+'.join(query)
	url= 'https://www.google.com/search?q='+query+'&source=lnms&tbm=isch'
	header={{ 'User-Agent':'Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.134 Safari/537.36'}}
	soup = get_soup(url, header)
	ActualImages =[]# contains the link for Large original images, type of  image
	for a in soup.find_all('div',{{ 'class':'rg_meta'}}):
		link , Type =json.loads(a.text)['ou']  ,json.loads(a.text)['ity']
		ActualImages.append((link, Type))
	return ActualImages
			
main(argv)
", _scope);

            var listLinkType = new List<Tuple<string, string>>();
            foreach (PythonTuple element in pythonResult)
                listLinkType.Add(new Tuple<string, string>(element.First().ToString(), element.Last().ToString()));

            var list = listLinkType.Select(x => x.Item1).Take(5);

            var dbUser = new CouchClient(Couch.EndPoint).GetDatabaseAsync(request.DbName).Result;
            var googleSearchImg = dbUser.GetAsync("googleSearchImg").Result;
            if (googleSearchImg.StatusCode == System.Net.HttpStatusCode.OK) {
                var googleSearchImgObj = JsonConvert.DeserializeObject<GoogleSearchImg>(googleSearchImg.Content);
                googleSearchImgObj.ListUrlImages = list.ToArray();
                dbUser.ForceUpdateAsync(JToken.FromObject(googleSearchImgObj));
            } else {
                var googleSearchImgObj = new GoogleSearchImg();
                googleSearchImgObj._id = "googleSearchImg";
                googleSearchImgObj.DbName = request.DbName;
                googleSearchImgObj.CqrsType = Cqrs.Query;
                googleSearchImgObj.ListUrlImages = list.ToArray();
                dbUser.ForceUpdateAsync(JToken.FromObject(googleSearchImgObj));
            }
        }
    }
}
