# Toaster

## Overview

### History

Toaster is a a unit test tool for .NET, specifically targeted at the Mono runtime.

I initially developed the Toaster toolkit while working on the Visual Studio IntelliTrace team at Microsoft in 2007.  I was attempting at the time to use the unit testing tools built in to Visual Studio (MSTest) to test the IntelliTrace product, and was becoming increasingly frustrated at the number of unwanted side-effects and features in MSTest.  The lack of control over how and where the test assemblies were staged, the craziness around using AppDomains to isolate tests, lack of access to environment variables, slowness, and so on.

What I wanted from a unit testing tool was minimalism.  Give me a simple API framework to write unit tests, make it run quickly, make the tests debuggable, give me easily parsed results. And then get out of my way.

Toaster is my solution to those problems. It is based on the MSTest framework, which in turn is a direct copy of the original NUnit framework.  I smoothed out some inconsistencies in the API's and added some useful stuff like test ordering and access to environment variables.

Some may consider the MSTest/NUnit API's to be a bit dated now.  For example, the more modern and very awesome [XUnit](https://xunit.codeplex.com/) eschews the use of initialize/cleanup methods and the need to use CLR attributes to identify test methods.  However, in my experience, none of the newer revised unit test tools gives you much that is different from tools that have been around for a decade or more.

In fact, the problem with the newer tools is that they often require a rewrite of existing tests.  You can convert MSTest and NUnit tests to Toaster very quickly and easily.

Another upside of Toaster over other unit test tools is that I believe you can write all 3 kinds of application tests using it:

- Unit tests
- System tests
- Stress tests

The following sections elaborate on each of these types of test.

### Unit Tests

I believe unit tests and unit test tools should adhere to the following rules:

- Be super easy to run after every build.
- Be really fast, like no more than a minute to run them all.
- They should use mock objects wherever possible.  Use a [dependency injection](http://en.wikipedia.org/wiki/Dependency_injection) framework and you will be fine.
- Be execution order independent.
- Be debuggable in a debugger.

Toaster gives you all these features.  The biggest downside right now is that Toaster doesn't have tight integration into Xamarin Studio.  

### System Tests

>Previously, I was using the word _functional_ here.  I didn't really like that word to describe this type of testing, so I've switched to _system_ instead.  You're testing the entire system instead of just a _unit_ of it.

When I wrote Toaster, I wanted a way to write system tests too.  By my definition, these are tests which work against an actual installed (or partially installed) instance of a product, versus units of code that are run in a sandboxed, mocked environment.  To me, the biggest major difference between unit testing and a system testing is the ability to run tests in a specific order, because setup time is non-trivial for system tests.  

Also, it is my contention that it is much, much easier to debug a problem in your logon code if you find out about it early, and not in the middle of your billing system tests 10 minutes later.  Specifically, system tests need to:

- Work against an actual installed version of the product.
- Often need to do significant setup and teardown.  Because of that they are...
- Order dependent.
- Longer running.  I'd say no more than 10 minutes is good goal.
- Still need to be debuggable in a debugger.

### Stress Tests

Finally, there are stress and "fuzz" tests.  These are tests where you simply throw scale and randomness at your application in an attempt to find problems.  These tests are:

- Long running.  Days is not unheard of.
- Not order dependent.
- Don't need to be debuggable in a debugger, but need access to really good logging.

## Installation in Xamarin Studio

My primary .NET development environment these days is [Xamarin Studio](http://xamarin.com/studio).  Here are instructions on how to install Toaster for use with it.

First, install the library package with [NuGet](http://www.nuget.org/packages/Toaster/) in the IDE.  You can also try installing the [Xamarin Studio Command Line Tools](http://lastexitcode.com/blog/2014/10/26/XamarinComponentsAndNuGet/) and installing the component that way.

Once you have the tools downloaded and in your `packages/` directory, you'll need a script to run the latest version of the `Toast.exe` tool.  I use this one, located in a `bin/` directory in my projects:

	#!/bin/bash
	#
	# A script to find the newest version of a tool in the NuGet packages directory
	#

	PKGNAME=Toaster
	TOOLNAME=Toast
	PKGDIR=../packages

	# See http://stackoverflow.com/questions/4493205/unix-sort-of-version-numbers
	
	mono $(find $PKGDIR -name $PKGNAME\.\* -type d | sed -Ee 's/^(.*-)([0-9.]+)(\.ime)$/\2.-1 \1\2\3/' | sort -t. -n -r -k1,1 -k2,2 -k3,3 -k4,4 | head -1)/tools/$TOOLNAME.exe $*

This script will always run the newest version of the `Toaster` tools that are installed for your projects.   You may need to adjust the `PKGDIR` for your project. 

Take a look at the `Tests/SampleTests` project for examples of all of all of the Toaster features. 

---

John Lyon-Smith, July 2014.
