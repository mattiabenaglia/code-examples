function CommentTableBase_Constructor(options) {
    /// <summary>Genera l'elemento che verrà associato al data del CommentTableBase.</summary>

    var self = this;

    self.cssClasses = {
        isfrompreviousquarter: 'isfrompreviousquarter'
    };

    self.options = _constructor_getOptionsExtendedWithDefaults(options);

    self.$element = _get_$element;

    self.tableData = _tableData;

    self.createDOM = _createDOM;

    self.add_draganddrop_tableHeader = _add_draganddrop_tableHeader;

    self.unshift_draganddrop_tableColumn = _unshift_draganddrop_tableColumn;

    self.add_draganddrop_tableRow = _add_draganddrop_tableRow;

    self.add_description_tableRow = _add_description_tableRow;

    self.id = kendo.guid();

    function _constructor_getOptionsExtendedWithDefaults(options) {

        var defaults = {
            domElement: null,
            cssClasses: null,
            columnsNamesList: null,
            isReadOnly: true,
            isFromPreviousQuarter: false,
            itemList: [],
            get_tableHeader: null,
            get_tableColumns: null,
            get_tableRow: null,
            textAreaCanSelect: false,
            addLine_extendedProperties: {}
        };

        return $.extend({}, defaults, options);
    }

    function _get_$element() {
        return $(self.options.domElement);
    }

    function _tableData() {
        return table.getData(_get_$element());
    }

    function _createDOM() {

        var $commentTableBase = _get_$element().addClass(self.options.cssClasses.commenttablebase);

        $commentTableBase.attr('id', self.id);

        $commentTableBase.table({
            columns: $.isFunction(self.options.get_tableColumns) === true ? self.options.get_tableColumns() : undefined,
            header: $.isFunction(self.options.get_tableHeader) === true ? self.options.get_tableHeader() : undefined,
            rows: _get_tableBase_tableRowsList()
        });

        if (self.options.isReadOnly !== true && self.options.isFromPreviousQuarter !== true) {
            $commentTableBase.after(_get_addLine());
            _set_dragAndSwap();
        }

        return $commentTableBase;

        function _get_tableBase_tableRowsList() {
            return $.map(self.options.itemList, _get_tableRow_correctFunction());
        }

        function _get_tableRow_correctFunction() {
            return $.isFunction(self.options.get_tableRow) === true ? self.options.get_tableRow : _get_tableBase_tableRow;
        }

        function _get_tableBase_tableRow(item) {
            /// <param name="item" type="Object">
            ///     id: 1,
            ///     description: ''
            ///     isFromPreviousQuarter: false
            /// </param >

            var obj = new Object();

            _add_draganddrop_tableRow(obj, item);

            _add_description_tableRow(obj, item);

            return obj;
        }

        function _get_addLine() {

            return button.createDOM({
                cssClasses: self.options.cssClasses.commenttablebase_addline,
                click: _addLine_options_click
            });

            function _addLine_options_click(event) {

                var tableData = _tableData(),
                    newRow = tableData.addRow(_get_tableRow_correctFunction()($.extend({
                        id: null,
                        description: '',
                        isFromPreviousQuarter: false
                    }, self.options.addLine_extendedProperties))),
                    $column_description = tableData.get$columnByRowAndName(newRow, self.options.columnsNamesList.tablebase_description),
                    $textArea = $column_description.find(textArea.selector.textArea);

                $textArea.focus();

                _set_dragAndSwap();
            }
        }

        function _set_dragAndSwap() {

            var selector = '#' + self.id + ' > .' + table.row.cssClass + ':not(.' + self.cssClasses.isfrompreviousquarter + ')';

            $(selector).dragAndSwap({
                draggable_handle: '.' + self.options.cssClasses.commenttablebase_draganddrop,
                droppable_accept: selector
            });
        }
    }

    function _add_draganddrop_tableHeader(table_header) {

        if (self.options.isReadOnly !== true) {
            table_header[self.options.columnsNamesList.tablebase_draganddrop] = {
                //cssClasses: '',
                html: ''
            };
        }
    }

    function _unshift_draganddrop_tableColumn(table_columns) {

        if (self.options.isReadOnly !== true) {
            table_columns.unshift({
                name: self.options.columnsNamesList.tablebase_draganddrop,
                minWidth: '1px',
                width: '30px'
            });
        }
    }

    function _add_draganddrop_tableRow(obj, item) {

        if (self.options.isReadOnly !== true && item.isFromPreviousQuarter !== true) {
            obj[self.options.columnsNamesList.tablebase_draganddrop] = _get_draganddrop_tableRow();
        }

        function _get_draganddrop_tableRow() {

            return {
                html: _get_draganddrop_tableRowHtml
            };

            function _get_draganddrop_tableRowHtml($column) {
                $column.append(button.createDOM({
                    cssClasses: self.options.cssClasses.commenttablebase_draganddrop
                }));
            }
        }
    }

    function _add_description_tableRow(obj, item) {

        obj[self.options.columnsNamesList.tablebase_description] = _get_description_tableRow();

        function _get_description_tableRow() {

            return {
                html: _get_description_tableRowHtml()
            };

            function _get_description_tableRowHtml() {

                if (self.options.isReadOnly === true || item.isFromPreviousQuarter === true) {
                    return _get_description_tableRowHtml_ReadOnly;
                }
                else {
                    return _get_description_tableRowHtml_Editable;
                }
            }

            function _get_description_tableRowHtml_ReadOnly($column) {

                $column.text({
                    id: item.id,
                    text: item.description,
                    additionalParams: {
                        sqlId: item.id,
                        isFromPreviousQuarter: item.isFromPreviousQuarter
                    }
                });
            }

            function _get_description_tableRowHtml_Editable($column) {

                var textAreaOptions = {
                    value: item.description,
                    isReadOnly: false,
                    maxLength: 800,
                    rows: 2,
                    cols: 1,
                    focusout: _textArea_focusout,
                    focusin: _textArea_focusin,
                    additionalParams: {
                        sqlId: item.id,
                        isFromPreviousQuarter: item.isFromPreviousQuarter
                    }
                };
                var $htmlToAppend = null;

                if (self.options.textAreaCanSelect === true) {
                    $htmlToAppend = textAreaSelectionModalDialog.createDOM({
                        popup: {
                            anchorId: 'PreviousCommentToSelect',
                            popUpTitle: vocabulary["PopUpTitle"],
                            popUpWidth: 800,
                            popUpHeight: 600,
                            title: vocabulary['ModalDialogTitle'],
                            pageUrl: kUtilities.navigation.calculateUrl('PopUp.aspx', 'id:' + self.options.id)
                        },
                        textArea: textAreaOptions,
                    });
                }
                else {
                    $htmlToAppend = textArea.createDOM(textAreaOptions);
                }

                $column.append($htmlToAppend);

                // aggiunta bottone di elimina textBoxArea
                var callbackAction = function () {
                    // funzione che permette di eliminare la riga dalla tabella
                    var rowToRemove = table.row.get$closest($htmlToAppend).get(0);
                    if (rowToRemove.firstChild.attributes.name.value === commentTable.columnsNamesList.comment_draganddrop) {

                        var commentArray = [];
                        $.each(rowToRemove.children[2].children, function (comment_idx, comment) {
                            commentArray.push({ commentDescription: comment.children[1].firstChild.value });
                        });

                        if (commentArray.length > 0) {
                            var atLeastOneNotEmptycomment = commentArray.filter(function (comment) {
                                return (kUtilities.commons.isNullUndefinedOrEmpty(comment.commentDescription) !== true);
                            });
                            if (atLeastOneNotEmptycomment.length > 0) {
                                rowToRemove.children[1].firstChild.firstChild.value = '';
                            }
                            else
                                _tableData().removeRow(rowToRemove);
                        }
                        else
                            _tableData().removeRow(rowToRemove);
                    }
                    else
                        _tableData().removeRow(rowToRemove);
                };

                var $deleteButton = _createdeleteButton(callbackAction);
                $column.append($deleteButton);
                // quando la textarea prende il focus vado a visualizzare il bottone X
                // quando la textarea perde il focus vado a nascondere il bottone X
                function _textArea_focusout() {
                    setTimeout(function () {
                        $deleteButton.hide();
                    }, 250);
                }
                function _textArea_focusin() {
                    $deleteButton.show();
                }
            }

            function _createdeleteButton(callbackAction) {
                // creo il div e aggiungo lo stile con l icona di elimina
                var $deleteButton = $(document.createElement('div'));
                $deleteButton.addClass('delete');
                // creo l'evento click sul bottone
                var _events = {
                    click: {
                        namespace: 'click.delete',
                        action: function () {
                            if ($.isFunction(callbackAction) == true) {
                                callbackAction.apply(this)
                            }
                        },
                        on: function () {
                            _events.click.off();
                            $deleteButton.on(_events.click.namespace, _events.click.action);
                        },
                        off: function () {
                            $deleteButton.off(_events.click.namespace);
                        }
                    }
                }
                _events.click.on();
                return $deleteButton;
            }
        }
    }
}
