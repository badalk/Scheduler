# Scheduler
Sitecore QuartzScheduler is a framework that allows you to schedule Quartz Job through a Sitecore SPEAK app. 

Here is the list of features

	- Bulit on Quartz.net - industry proven enterprise job scheduler
	- Manage and Define Jobs and when those jobs will trigger (according to server clock time and not app pool recycle sliding time)
		○ Define triggers based on duration every few hours, minutes or seconds
		○ Define triggers to fire daily
		○ Define triggers that fire only certain days of the week 
		○ Define triggers that fire monthly once on any given day
		○ If none-of the above suits your need - Define custom triggers based on cron expression guidelines of Quartz.net
	- Job Performance Dashboard showing 
		○ Job Performance Average and Maximum time it has taken
		○ Individual Job Performance - from last app pool recycle
		○ Current Execution Status and next fire times for each job trigger defined
	- Execute a job on-demand (instead of waiting for schedule to trigger)
	- Download job performance data as a Json file (in the event you are recycling the app pool and want to retain this data before you do so)
	- Out of the box jobs for 
		○ Cleanup Publish Queue
		○ Cleanup Event Queue
		○ Cleanup History
	- You can create your own jobs and create definition items in Sitecore through Sitecore SPEAK app
	- Extendible - create your own trigger statistics store to store performance benchmarks for longer term rather than in application cache

Copyright (c) 2016 Badal Kotecha
