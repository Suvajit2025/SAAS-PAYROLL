/*
Name: 			Tables Logs
Written by: 	Clara Garnier - claragarnier.com
Theme Version: 	1.0.0 - TTMS
*/

(function( $ ) {

	'use strict';

	var datatableInit = function() {

		$('#datatable-logs').dataTable(
		
		{
		"filter":false,
		"ordering":false,
        "info":false ,
        "bLengthChange": false,
        "iDisplayLength": 100      
        
    	});
		
	};
	
	
		

	$(function() {
		datatableInit();
	});

}).apply( this, [ jQuery ]);



(function( $ ) {

	'use strict';

	var datatableInit = function() {

		$('#datatable-available').dataTable(
		
		{
		"filter":true,
		"ordering":true,
        "info":false ,
        "bLengthChange": true,
        "bAutoWidth" : false,
        "iDisplayLength": 50      
        
    	});
		
	};
	
	
		

	$(function() {
		datatableInit();
	});

}).apply( this, [ jQuery ]);

//DVIR

(function( $ ) {

	'use strict';

	var datatableInit = function() {

		$('#datatable-dvir').dataTable(
		
		{
		"filter":false,
		"ordering":true,
        "info":false ,
        "bLengthChange": false,
        "bAutoWidth" : false,
        "iDisplayLength": 50      
        
    	});
		
	};
	
	
		

	$(function() {
		datatableInit();
	});

}).apply( this, [ jQuery ]);

(function( $ ) {

	'use strict';

	var datatableInit = function() {

		$('#datatable-dvir1').dataTable(
		
		{
		"filter":false,
		"ordering":true,
        "info":false ,
        "bLengthChange": false,
        "bPaginate": false,
        "bAutoWidth" : false,
        "iDisplayLength": 10      
        
    	});
		
	};
	
	
		

	$(function() {
		datatableInit();
	});

}).apply( this, [ jQuery ]);

(function( $ ) {

	'use strict';

	var datatableInit = function() {

		$('#datatable-trip').dataTable(		
		{
			
		"filter":false,
		"ordering":false,
        "info":false ,
        "bLengthChange": false,
		"autoWidth": false,
		 "aoColumns": [
            { sWidth: '10%' },
            { sWidth: '10%' },
            { sWidth: '12%' },
            { sWidth: '15%' },
            { sWidth: '12%' },
            { sWidth: '15%' },
            { sWidth: '8%' },
            { sWidth: '8%' },
            { sWidth: '10%' } ]          
    	});
		
	};
	
	$(function() {
		datatableInit();
	});

}).apply( this, [ jQuery ]);

(function( $ ) {

	'use strict';

	var datatableInit = function() {

		$('#datatable-log').dataTable(		
		{
			
		"filter":false,
		"ordering":false,
        "info":false ,
        "bLengthChange": false,
          
    	});
		
	};
	
	$(function() {
		datatableInit();
	});

}).apply( this, [ jQuery ]);


