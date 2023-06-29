/*
Name: 			Live trace stats
Written by: 	
Theme Version: 	
*/

(function( $ ) {

	'use strict';

$('#running').liquidMeter({
			shape: 'circle',
			color: '#bacf58',
			background: '#F9F9F9',
			fontSize: '24px',
			fontWeight: '600',
			stroke: '#F2F2F2',
			textColor: '#333',
			liquidOpacity: 0.9,
			liquidPalette: ['#333'],
			speed: 3000,
			animate: !$.browser.mobile
		});
		
		
$('#idle').liquidMeter({
			shape: 'circle',
			color: '#ffbf2b',
			background: '#F9F9F9',
			fontSize: '24px',
			fontWeight: '600',
			stroke: '#F2F2F2',
			textColor: '#333',
			liquidOpacity: 0.9,
			liquidPalette: ['#333'],
			speed: 3000,
			animate: !$.browser.mobile
		});
		
		$('#stopped').liquidMeter({
			shape: 'circle',
			color: '#fb4f4f',
			background: '#F9F9F9',
			fontSize: '24px',
			fontWeight: '600',
			stroke: '#F2F2F2',
			textColor: '#333',
			liquidOpacity: 0.9,
			liquidPalette: ['#333'],
			speed: 3000,
			animate: !$.browser.mobile
		});
		
		$('#others').liquidMeter({
			shape: 'circle',
			color: '#9C3B6B',
			background: '#F9F9F9',
			fontSize: '24px',
			fontWeight: '600',
			stroke: '#F2F2F2',
			textColor: '#333',
			liquidOpacity: 0.9,
			liquidPalette: ['#333'],
			speed: 3000,
			animate: !$.browser.mobile
		});

	}).apply( this, [ jQuery ]);