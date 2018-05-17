var table = {

    cssClass: 'table',
    dataName: 'table',
    getData: function ($table) {
        return $table.data(table.dataName);
    },

    Constructor: function (options) {
        /// <summary>Genera l'elemento che verrà associato al data del table.</summary>
        /// <param name="options" type="Object">
        ///     domElement: null
        ///     //vedi $.fn.table
        /// </param>

        var self = this;
        
        self.options = _constructor_getOptionsExtendedWithDefaults();
        
        self.$element = _$element;
        
        //#region Columns
        
        self.get$columnsByName = _get$columnsByName;

        self.get$emptyColumnsByName = _get$emptyColumnsByName;

        self.get$firstEmptyColumnsByName = _get$firstEmptyColumnsByName;

        //#endregion
        
        //#region Rows
        
        self.addRow = _addRow;

        self.addRowList = _addRowList;

        self.removeRow = _removeRow;

        self.get$columnByRowAndName = _get$columnByRowAndName;

        //#endregion

        self.get_$groupsAndRows = _get_$groupsAndRows;
        
        self.createDOM = _createDOM;

        self.destroy = _destroy;

        function _constructor_getOptionsExtendedWithDefaults() {

            var defaults = {
                domElement: null,
                columns: [],
                header: null,
                rows: [],
                cssClasses: null
            };

            return $.extend({}, defaults, options);
        }

        function _$element() {
            return $(self.options.domElement);
        }

        function _setCssClasses($element, cssClasses) {
            if (kUtilities.commons.isNullUndefinedOrEmpty(cssClasses) != true) {
                $element.addClass(cssClasses);
            }
        }

        function _getColumnsSelectorByName_WithRow(name) {
            return ' > .' + table.row.cssClass + _getColumnsSelectorByName(name);
        }

        function _getColumnsSelectorByName(name) {
            return ' > .' + table.column.cssClass + '[name="' + name + '"]';
        }

        function _get$columnsByName(name) {
            return self.$element().find(_getColumnsSelectorByName_WithRow(name));
        }

        function _get$emptyColumnsByName(name) {
            return self.$element().find(_getColumnsSelectorByName_WithRow(name) + ':empty');
        }

        function _get$firstEmptyColumnsByName(name) {
            return self.$element().find(_getColumnsSelectorByName_WithRow(name) + ':empty:first');
        }
        
        function _add_column($element, options) {

            var $column = $('<div></div>')
                .addClass(table.column.cssClass)
                .attr('name', options.name)
                .css({
                    'min-width': options.minWidth,
                    'width': options.width
                });

            $element.append($column);

            _setCssClasses($column, options.cssClasses);

            $column.html($.isFunction(options.html) ? options.html($column) : options.html);

        }
        
        function _add_header($table, options) {

            if (kUtilities.commons.isNullUndefinedOrEmpty(options) != true) {

                var $header = $('<div></div>').addClass(table.header.cssClass);
                $table.append($header);

                _setCssClasses($header, options.cssClasses);

                $.map(self.options.columns, function (columnOptions) {
                    return _add_column($header, $.extend({}, columnOptions, options[columnOptions.name]));
                });
            }
        }
        
        function _add_group($table, options) {

            if (kUtilities.commons.isNullUndefinedOrEmpty(options) != true) {

                var $group = $('<div></div>').addClass(table.group.cssClass);
                $table.append($group);

                _setCssClasses($group, options.cssClasses);

                $group.html($.isFunction(options.html) ? options.html($group) : options.html);

                _addList_row($table, options.rows);
            }
        }

        function _addList_group($table, groupListOptions) {

            if (kUtilities.commons.isNullUndefinedOrEmpty(groupListOptions) != true) {
                $.each(groupListOptions, function (groupIdx, groupOptions) {
                    _add_group($table, groupOptions);
                });
            }

        }

        function _addRow(options) {
            return _add_row(self.$element(), options);
        }

        function _add_row($table, options) {


            if (kUtilities.commons.isNullUndefinedOrEmpty(options) != true) {

                var $row = $('<div></div>').addClass(table.row.cssClass);
                $table.append($row);

                _setCssClasses($row, options.cssClasses);

                $.map(self.options.columns, function (columnOptions) {
                    return _add_column($row, $.extend({}, columnOptions, options[columnOptions.name]));
                });

                return $row;
            }
        }

        function _addRowList(rowListOptions) {
            _addList_row(self.$element(), rowListOptions);
        }

        function _addList_row($table, rowListOptions) {

            if (kUtilities.commons.isNullUndefinedOrEmpty(rowListOptions) != true) {
                $.each(rowListOptions, function (rowIdx, rowOptions) {
                    _add_row($table, rowOptions);
                });
            }

        }
        
        function _removeRow(domRowToDelete) {
            $(domRowToDelete).remove();
        }

        function _get$columnByRowAndName(row, name) {
            return $(row).find(_getColumnsSelectorByName(name));
        }

        function _get_$groupsAndRows() {
            return self.$element().find('> .' + table.group.cssClass + ', > .' + table.row.cssClass);
        }

        function _createDOM() {

            var $table = self.$element().addClass(table.cssClass);
            if (kUtilities.commons.isNullUndefinedOrEmpty(self.options.cssClasses) === false) {
                self.$element().addClass(self.options.cssClasses);
            }

            _setTotMinWidth(self.options);

            _add_header($table, self.options.header);

            _addList_group($table, self.options.groups);

            _addList_row($table, self.options.rows);

            return $table;
        }


        function _destroy() {

            var $table = self.$element();

            $table.removeClass($table.cssClass);

            var allRowsAndGroups = table.getData($table).get_$groupsAndRows();

            $.each(allRowsAndGroups, function (idx, row) {
                $(row).remove();
            });

            $table.removeData(table.dataName);
        }

        function _setTotMinWidth($table) {
            var totMinWidth = 0;
            $.each(self.options.columns, function (idx, column) {
                totMinWidth += uiParseNumber(column.minWidth.replace('px', ''));
            });
            //console.log($table.css())

            //$table.css({
            //    'min-width': totMinWidth
            //});
        }
    },
    
    column: {
        cssClass: 'table-column'
    },
    header: {
        cssClass: 'table-header'
    },
    group: {
        cssClass: 'table-group'
    },
    row: {
        cssClass: 'table-row',
        get$closest: function ($child) {
            return $child.closest('.' + table.row.cssClass);
        }
    }
};

$.fn.table = function (options) {
    /// <summary>Inizializza la lista di commenti.</summary>
    /// <param name="options" type="Object">
    ///     columns: [{
    ///         name: '',
    ///         cssClasses: '',
    ///         minWidth: '',
    ///         width: ''
    ///     }],
    ///      header: {
    ///         cssClasses: '',
    ///         columnName: {
    ///             cssClasses: '',
    ///             html: '' / function () { }
    ///         }
    ///     },
    ///     
    ///     //Se venissero settate entrambe le proprietà rows e groups, le rows andrebbero accodate alle rows dell'ultimo group
    ///     rows: [{
    ///         columnName: {
    ///             cssClasses: '',
    ///             html: '' / function () { }
    ///         }
    ///     }],
    ///     groups: [{
    ///         cssClasses: '',
    ///         html: ''/function () {},
    ///         rows: [{
    ///             //vedi rows
    ///         }],
    ///     
    ///    cssClasses:''
    ///    
    /// </param>
    
    var $table = this;

    $table.each(function (idx, domtable) {
        var $this = $(domtable);

        $this.data(table.dataName, new table.Constructor($.extend(options, {
            domElement: domtable
        })));


        var tableData = $this.data(table.dataName);

        tableData.createDOM();
    });

    return $table;
};