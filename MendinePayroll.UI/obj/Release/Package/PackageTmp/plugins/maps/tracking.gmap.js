/*
Name: 			Maps / Basic - Examples
Written by: 	Clara Garnier - claragarnier.com
Theme Version: 	1.0.0 - TTMS
*/

(function( $ ) {

	'use strict';

	var initBasic = function() {
		new GMaps({
			div: '#gmap-basic',
			lat: 34.894942,
			lng: -99.023439,
			zoomControlOptions: {
                style: google.maps.ZoomControlStyle.SMALL
            },
			zoom:5
		});
	};

	var initBasicWithMarkers = function() {
		var map = new GMaps({
			div: '#gmap-basic-marker',
			lat: 30.031055,
			lng: -95.343019,
			markers: [{
				lat: 30.031055,
				lng: -95.343019,
				infoWindow: {
					content: '<p>Basic</p>'
				}
			}]
		});

		map.addMarker({
			lat: 30.031055,
			lng: -95.343019,
			infoWindow: {
				content: '<p>Example</p>'
			}
		});
	};

	var initStatic = function() {
		var url = GMaps.staticMapURL({
			size: [725, 500],
			lat: 30.031055,
			lng: -95.343019,
			scale: 1
		});

		$('#gmap-static')
			.css({
				backgroundImage: 'url(' + url + ')',
				backgroundSize: 'cover'
			});
	};

	var initContextMenu = function() {
		var map = new GMaps({
			div: '#gmap-context-menu',
			lat: -12.043333,
			lng: -77.028333
		});

		map.setContextMenu({
			control: 'map',
			options: [
				{
					title: 'Add marker',
					name: 'add_marker',
					action: function(e) {
						this.addMarker({
							lat: e.latLng.lat(),
							lng: e.latLng.lng(),
							title: 'New marker'
						});
					}
				},
				{
					title: 'Center here',
					name: 'center_here',
					action: function(e) {
						this.setCenter(e.latLng.lat(), e.latLng.lng());
					}
				}
			]
		});
	};

	var initStreetView = function() {
		var gmap = GMaps.createPanorama({
			el: '#gmap-street-view',
			lat : 48.85844,
			lng : 2.294514
		});

		$(window).on( 'sidebar-left-toggle', function() {
			google.maps.event.trigger( gmap, 'resize' );
		});
	};

	// auto initialize
	$(function() {

		initBasic();
		initBasicWithMarkers();
		initStatic();
		initContextMenu();
		initStreetView();

	});
	
	
	        (function() {
          var map = new GMaps({
            div: '#markersGmap',
            lat: 13.0127,
            lng: 77.55632
          });
          map.addMarker({
            lat: 13.0127,
            lng: 77.55632,
            title: 'Tracking',
            infoWindow: {
              content: '<p>Destination</p>'
            }
          });
          map.addMarker({
            lat: 13.011153,
            lng: 77.556774,
            title: 'Marker with InfoWindow',
            infoWindow: {
              content: '<p>Trip Started</p>'
            }
          });
        })();
        
        // Simple
        // ------------------
        (function() {
          var simpleMap = new GMaps({
            el: '#simpleGmap',
            zoom: 8,
            center: {
              lat: -34.397,
              lng: 150.644
            }
          });
        })();
        
        

}).apply(this, [ jQuery ]);
