var commentsTable = {

    cssClasses: {
        commentsTable: 'commentsTable',
        commentsTable_draganddrop: 'commentsTable-draganddrop',
        commentsTable_addline: 'commentsTable-addline'
    },
    dataName: 'commentsTable',
    getData: function ($table) {
        return $table.data(commentsTable.dataName);
    },

    columnsNamesList: {
        comment_draganddrop: 'comment-draganddrop',
        comment_description: 'comment-description'
    },

    Constructor: function commentsTable_Constructor(options) {
        /// <summary>Genera l'elemento che verrà associato al data del commentsTable.</summary>
        /// <param name="options" type="Object">
        ///     domElement: null
        ///     //vedi $.fn.commentsTable
        /// </param>

        var self = this;

        self.options = _constructor_getOptionsExtendedWithDefaults(options);

        var commentTableBase = new CommentTableBase_Constructor({
            domElement: self.options.domElement,
            cssClasses: {
                commenttablebase: commentsTable.cssClasses.commentsTable,
                commenttablebase_draganddrop: commentsTable.cssClasses.commentsTable_draganddrop,
                commenttablebase_addline: commentsTable.cssClasses.commentsTable_addline
            },
            columnsNamesList: {
                tablebase_draganddrop: commentsTable.columnsNamesList.comment_draganddrop,
                tablebase_description: commentsTable.columnsNamesList.comment_description
            },
            isReadOnly: self.options.isReadOnly,
            itemList: $.map(self.options.commentList, _map_commentList),
            get_tableHeader: _get_tableHeader,
            get_tableColumns: _get_tableColumns,
            textAreaCanSelect: true
        });
        
        self.createDOM = _createDOM;

        self.getValue = _getValue;

        function _constructor_getOptionsExtendedWithDefaults(options) {

            var defaults = {
                domElement: null,
                isReadOnly: true,
                commentList: null
            };

            return $.extend({}, defaults, options);
        }

        function _map_commentList(comment) {
            return {
                id: comment.id,
                description: comment.desc
            };
        }

        function _get_tableHeader() {

            var table_header = {
                cssClasses: ''
            };

            commentTableBase.add_draganddrop_tableHeader(table_header);

            table_header[commentsTable.columnsNamesList.comment_description] = {
                //cssClasses: '',
                html: vocabulary.commentHeader
            };
            
            return table_header;
        }

        function _get_tableColumns() {
            var table_columns = [{
                name: commentsTable.columnsNamesList.comment_description,
                minWidth: '1px',
                width: 'calc(98.5% - 30px)'
            }];

            commentTableBase.unshift_draganddrop_tableColumn(table_columns);

            return table_columns;
        }
        
        function _createDOM() {
            commentTableBase.createDOM();
        }
        
        function _getValue() {

            var tableData = commentTableBase.tableData();

            return $.map(tableData.get_$groupsAndRows(), _getValue);

            function _getValue(row, idx) {
                
                var $column_description = tableData.get$columnByRowAndName(row, commentsTable.columnsNamesList.comment_description),
                    textAreaData_description = textArea.getData($column_description.find(textArea.selector.textArea));

                return {
                    idcomment: textAreaData_description.options.additionalParams.sqlId,
                    commentDesc: textAreaData_description.getValue(),
                    sort: idx
                };
            }
        }
    }
};

$.fn.commentsTable = function (options) {
    /// <summary>Inizializza la tabella di commenti.</summary>
    /// <param name="options" type="Object">
    ///     isReadOnly: true,
    ///     idcomment: 1,
    ///     commentList:[]
    /// </param>

    var $commentsTable = this;
    $commentsTable.each(function (idx, domcommentsTable) {  

        var $this = $(domcommentsTable);

        $this.data(commentsTable.dataName, new commentsTable.Constructor($.extend(options, {
            domElement: domcommentsTable
        })));

        var commentsTableData = $this.data(commentsTable.dataName);

        commentsTableData.createDOM();
    });

    return $commentsTable;
};

